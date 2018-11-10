using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class TryPromoteFromWaitingListCommandHandler : IRequestHandler<TryPromoteFromWaitingListCommand>
    {
        private readonly ImbalanceManager _imbalanceManager;
        private readonly ILogger _log;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IRepository<Registration> _registrations;
        private readonly IRepository<Seat> _seats;
        private readonly ServiceBusClient _serviceBusClient;

        public TryPromoteFromWaitingListCommandHandler(IQueryable<Registrable> registrables,
                                                       IRepository<Seat> seats,
                                                       IRepository<Registration> registrations,
                                                       ImbalanceManager imbalanceManager,
                                                       ServiceBusClient serviceBusClient,
                                                       ILogger log)
        {
            _registrables = registrables;
            _seats = seats;
            _registrations = registrations;
            _imbalanceManager = imbalanceManager;
            _serviceBusClient = serviceBusClient;
            _log = log;
        }

        public async Task<Unit> Handle(TryPromoteFromWaitingListCommand command, CancellationToken cancellationToken)
        {
            var registrationIdsToCheck = new List<Guid?>();
            var registrableToCheck = await _registrables.Where(rbl => rbl.Id == command.RegistrableId
                                                                   && rbl.EventId == command.EventId)
                                                        .Include(rbl => rbl.Seats)
                                                        .FirstOrDefaultAsync(cancellationToken);
            registrationIdsToCheck.AddRange(await TryPromoteFromRegistrableWaitingList(registrableToCheck));

            foreach (var registrationId in registrationIdsToCheck.Where(id => id.HasValue).Select(id => id.Value))
            {
                _serviceBusClient.SendMessage(new CheckIfRegistrationIsPromotedCommand { RegistrationId = registrationId });
            }

            return Unit.Value;
        }

        private IEnumerable<Guid?> AcceptPartnerSeatsFromWaitingList(Registrable registrable, int seatsLeft)
        {
            var seatsToAccept = registrable.Seats
                                           .Where(seat => seat.IsWaitingList && !seat.IsCancelled)
                                           .OrderByDescending(seat => seat.RegistrationId.HasValue && seat.RegistrationId_Follower.HasValue)
                                           .ThenBy(seat => seat.FirstPartnerJoined);
            foreach (var seat in seatsToAccept)
            {
                if (!seat.IsPartnerSpot)
                {
                    // single registration, check imbalance
                    var ownRole = seat.RegistrationId.HasValue ? Role.Leader : Role.Follower;
                    if (!_imbalanceManager.CanAddNewDoubleSeatForSingleRegistration(registrable, ownRole))
                    {
                        continue;
                    }
                }
                seat.IsWaitingList = false;
                if (seat.RegistrationId.HasValue)
                {
                    yield return seat.RegistrationId;
                }
                if (seat.RegistrationId_Follower.HasValue)
                {
                    yield return seat.RegistrationId_Follower;
                }
                if (--seatsLeft <= 0)
                {
                    yield break;
                }
            }
        }

        private async Task<IEnumerable<Guid?>> AcceptSingleSeatsFromWaitingList(IEnumerable<Seat> seatsToAccept)
        {
            var registrationIdsToCheck = new List<Guid?>();
            foreach (var seat in seatsToAccept)
            {
                seat.IsWaitingList = false;
                await SetRegistrationNotOnWaitingListAnymore(seat.RegistrationId);
                registrationIdsToCheck.Add(seat.RegistrationId);
            }
            return registrationIdsToCheck;
        }

        private async Task SetRegistrationNotOnWaitingListAnymore(Guid? registrationId)
        {
            if (!registrationId.HasValue)
            {
                return;
            }

            var registration = await _registrations.FirstOrDefaultAsync(reg => reg.Id == registrationId);

            if (registration == null)
            {
                _log.LogInformation($"No registration found with id {registrationId}");
            }
            else
            {
                registration.IsWaitingList = false;
                if (!registration.AdmittedAt.HasValue)
                {
                    registration.AdmittedAt = DateTime.UtcNow;
                }
            }
        }

        private async Task<IEnumerable<Guid?>> TryPromoteFromRegistrableWaitingList(Registrable registrable)
        {
            var registrationIdsToCheck = new List<Guid?>();
            var seats = registrable.Seats.Where(seat => !seat.IsCancelled).ToList();
            if (registrable.MaximumSingleSeats.HasValue)
            {
                var acceptedSeatCount = seats.Count(seat => !seat.IsWaitingList);
                var seatsLeft = registrable.MaximumSingleSeats.Value - acceptedSeatCount;
                if (seatsLeft > 0)
                {
                    return await AcceptSingleSeatsFromWaitingList(seats.Where(seat => seat.IsWaitingList)
                                                                       .OrderBy(seat => seat.FirstPartnerJoined)
                                                                       .Take(seatsLeft));
                }
            }
            else if (registrable.MaximumDoubleSeats.HasValue)
            {
                // try match single leaders and followers
                var singleSeats = seats.Where(seat => !seat.IsPartnerSpot).ToList();
                var acceptedSingleLeaders = singleSeats.Where(seat => !seat.IsWaitingList &&
                                                                      !seat.RegistrationId_Follower.HasValue)
                                                       .ToList();
                var acceptedSingleFollowers = singleSeats.Where(seat => !seat.IsWaitingList &&
                                                                        !seat.RegistrationId.HasValue)
                                                         .ToList();

                var waitingFollowers = new Queue<Seat>(singleSeats.Where(seat => seat.IsWaitingList &&
                                                                                 !seat.RegistrationId.HasValue)
                                                                  .OrderBy(seat => seat.FirstPartnerJoined)
                                                                  .ToList());
                var waitingLeaders = new Queue<Seat>(singleSeats.Where(seat => seat.IsWaitingList &&
                                                                               !seat.RegistrationId_Follower.HasValue)
                                                                .OrderBy(seat => seat.FirstPartnerJoined)
                                                                .ToList());
                _log.LogInformation($"Registrable {registrable.Name}, single leaders in {acceptedSingleLeaders.Count}, single followers in {acceptedSingleFollowers.Count}, single leaders waiting {waitingLeaders.Count}, single followers waiting {waitingFollowers.Count}");
                var singleRegistrationsPromoted = false;
                if (acceptedSingleLeaders.Any() && waitingFollowers.Any())
                {
                    foreach (var acceptedSingleLeader in acceptedSingleLeaders)
                    {
                        if (!waitingFollowers.Any())
                        {
                            // no more waiting followers
                            break;
                        }
                        var waitingFollower = waitingFollowers.Dequeue();
                        var registrationId = waitingFollower.RegistrationId_Follower;
                        acceptedSingleLeader.RegistrationId_Follower = registrationId;
                        _seats.Remove(waitingFollower);
                        registrationIdsToCheck.Add(registrationId);
                        await SetRegistrationNotOnWaitingListAnymore(registrationId);
                        singleRegistrationsPromoted = true;
                    }
                }

                if (acceptedSingleFollowers.Any() && waitingLeaders.Any())
                {
                    foreach (var acceptedSingleFollower in acceptedSingleFollowers)
                    {
                        if (!waitingLeaders.Any())
                        {
                            // no more waiting followers
                            break;
                        }
                        var waitingLeader = waitingLeaders.Dequeue();
                        var registrationId = waitingLeader.RegistrationId;
                        acceptedSingleFollower.RegistrationId = registrationId;
                        _seats.Remove(waitingLeader);
                        registrationIdsToCheck.Add(registrationId);
                        await SetRegistrationNotOnWaitingListAnymore(registrationId);
                        singleRegistrationsPromoted = true;
                    }
                }

                // be defensive - registrable.Seats might be inconsistent with the actual seats on the db
                if (!singleRegistrationsPromoted)
                {
                    // try promote partner registration
                    // ToDo: precedence partner vs. single registrations
                    var acceptedSeatCount = seats.Count(seat => !seat.IsWaitingList);
                    var seatsLeft = registrable.MaximumDoubleSeats.Value - acceptedSeatCount;
                    if (seatsLeft > 0)
                    {
                        return AcceptPartnerSeatsFromWaitingList(registrable, seatsLeft);
                    }
                }
            }
            return registrationIdsToCheck;
        }
    }
}