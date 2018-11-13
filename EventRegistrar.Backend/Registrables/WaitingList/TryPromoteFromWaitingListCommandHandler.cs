using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class TryPromoteFromWaitingListCommandHandler : IRequestHandler<TryPromoteFromWaitingListCommand>
    {
        private readonly EventBus _eventBus;
        private readonly ImbalanceManager _imbalanceManager;
        private readonly ILogger _log;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IRepository<Seat> _spots;

        public TryPromoteFromWaitingListCommandHandler(IQueryable<Registrable> registrables,
                                                       IRepository<Seat> spots,
                                                       ImbalanceManager imbalanceManager,
                                                       EventBus eventBus,
                                                       ILogger log)
        {
            _registrables = registrables;
            _spots = spots;
            _imbalanceManager = imbalanceManager;
            _eventBus = eventBus;
            _log = log;
        }

        public async Task<Unit> Handle(TryPromoteFromWaitingListCommand command, CancellationToken cancellationToken)
        {
            var registrableToCheck = await _registrables.Where(rbl => rbl.Id == command.RegistrableId
                                                                   && rbl.EventId == command.EventId)
                                                        .Include(rbl => rbl.Seats)
                                                        .FirstOrDefaultAsync(cancellationToken);
            var spots = registrableToCheck.Seats.Where(spt => !spt.IsCancelled).ToList();
            if (registrableToCheck.MaximumSingleSeats.HasValue)
            {
                var acceptedSpotCount = spots.Count(spt => !spt.IsWaitingList);
                var spotsAvailable = registrableToCheck.MaximumSingleSeats.Value - acceptedSpotCount;
                if (spotsAvailable > 0)
                {
                    var spotsToPromote = spots.Where(spt => spt.IsWaitingList)
                                              .OrderBy(spt => spt.FirstPartnerJoined)
                                              .Take(spotsAvailable);
                    foreach (var spot in spotsToPromote)
                    {
                        await PromoteSpotFromWaitingList(spot);
                    }
                }
            }
            else if (registrableToCheck.MaximumDoubleSeats.HasValue)
            {
                // try match single leaders and followers (fill the gaps)
                var singleSpots = spots.Where(spt => !spt.IsPartnerSpot).ToList();
                var acceptedSingleLeaders = singleSpots.Where(spt => !spt.IsWaitingList
                                                                  && spt.RegistrationId != null
                                                                  && spt.RegistrationId_Follower == null)
                                                       .ToList();
                var acceptedSingleFollowers = singleSpots.Where(spt => !spt.IsWaitingList
                                                                    && spt.RegistrationId == null
                                                                    && spt.RegistrationId_Follower != null)
                                                         .ToList();

                var waitingFollowers = new Queue<Seat>(singleSpots.Where(spt => spt.IsWaitingList
                                                                             && spt.RegistrationId == null
                                                                             && spt.RegistrationId_Follower != null)
                                                                  .OrderBy(spt => spt.FirstPartnerJoined)
                                                                  .ToList());
                var waitingLeaders = new Queue<Seat>(singleSpots.Where(spt => spt.IsWaitingList
                                                                           && spt.RegistrationId != null
                                                                           && spt.RegistrationId_Follower == null)
                                                                .OrderBy(spt => spt.FirstPartnerJoined)
                                                                .ToList());
                _log.LogInformation($"Registrable {registrableToCheck.Name}, single leaders in {acceptedSingleLeaders.Count}, single followers in {acceptedSingleFollowers.Count}, single leaders waiting {waitingLeaders.Count}, single followers waiting {waitingFollowers.Count}");
                var singleRegistrationsPromoted = false;
                if (acceptedSingleLeaders.Any() && waitingFollowers.Any())
                {
                    foreach (var acceptedSingleLeader in acceptedSingleLeaders)
                    {
                        if (!waitingFollowers.Any())
                        {
                            // no more waiting single followers
                            break;
                        }
                        var waitingFollower = waitingFollowers.Dequeue();
                        var promotedRegistrationId = waitingFollower.RegistrationId_Follower;
                        if (promotedRegistrationId == null)
                        {
                            continue;
                        }

                        acceptedSingleLeader.RegistrationId_Follower = promotedRegistrationId;
                        waitingFollower.IsCancelled = true;
                        await _spots.InsertOrUpdateEntity(acceptedSingleLeader, cancellationToken);
                        await _spots.InsertOrUpdateEntity(waitingFollower, cancellationToken);

                        _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                        {
                            Id = Guid.NewGuid(),
                            RegistrableId = acceptedSingleLeader.RegistrableId,
                            RegistrationId = promotedRegistrationId.Value
                        });

                        singleRegistrationsPromoted = true;
                    }
                }

                if (acceptedSingleFollowers.Any() && waitingLeaders.Any())
                {
                    foreach (var acceptedSingleFollower in acceptedSingleFollowers)
                    {
                        if (!waitingLeaders.Any())
                        {
                            // no more waiting single leaders
                            break;
                        }
                        var waitingLeader = waitingLeaders.Dequeue();
                        var promotedRegistrationId = waitingLeader.RegistrationId;
                        if (promotedRegistrationId == null)
                        {
                            continue;
                        }

                        acceptedSingleFollower.RegistrationId = promotedRegistrationId;
                        waitingLeader.IsCancelled = true;
                        await _spots.InsertOrUpdateEntity(acceptedSingleFollower, cancellationToken);
                        await _spots.InsertOrUpdateEntity(waitingLeader, cancellationToken);

                        _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                        {
                            Id = Guid.NewGuid(),
                            RegistrableId = acceptedSingleFollower.RegistrableId,
                            RegistrationId = promotedRegistrationId.Value
                        });

                        singleRegistrationsPromoted = true;
                    }
                }

                // be defensive - registrable.Seats might be different than the actual seats on the db
                if (!singleRegistrationsPromoted)
                {
                    // try promote partner registration
                    // ToDo: precedence partner vs. single registrations
                    var acceptedSpotCount = spots.Count(spt => !spt.IsWaitingList);
                    var spotsAvailable = registrableToCheck.MaximumDoubleSeats.Value - acceptedSpotCount;
                    if (spotsAvailable > 0)
                    {
                        await AcceptSpotsFromPartnerWaitingList(registrableToCheck, spotsAvailable);
                    }
                }
            }

            return Unit.Value;
        }

        private async Task AcceptSpotsFromPartnerWaitingList(Registrable registrable, int spotsAvailable)
        {
            var spotsToAccept = registrable.Seats
                                           .Where(spt => spt.IsWaitingList
                                                      && !spt.IsCancelled)
                                           .OrderByDescending(spt => spt.RegistrationId.HasValue
                                                                  && spt.RegistrationId_Follower.HasValue)
                                           .ThenBy(spt => spt.FirstPartnerJoined);
            foreach (var spt in spotsToAccept)
            {
                if (spt.IsPartnerSpot)
                {
                    await PromoteSpotFromWaitingList(spt);
                }
                else
                {
                    // single registration, check imbalance
                    var ownRole = spt.RegistrationId.HasValue ? Role.Leader : Role.Follower;
                    if (!_imbalanceManager.CanAddNewDoubleSeatForSingleRegistration(registrable, ownRole))
                    {
                        // no promotion due to imbalance
                        continue;
                    }
                    await PromoteSpotFromWaitingList(spt);
                }
                if (--spotsAvailable <= 0)
                {
                    break;
                }
            }
        }

        private async Task PromoteSpotFromWaitingList(Seat spot)
        {
            var registrationId = spot.RegistrationId ?? spot.RegistrationId_Follower;
            if (registrationId == null)
            {
                _log.LogWarning("Unexpected situation: Spot {0} has neither leader nor follower set", spot.Id);
                return;
            }

            spot.IsWaitingList = false;
            await _spots.InsertOrUpdateEntity(spot);
            if (spot.IsPartnerSpot)
            {
                _eventBus.Publish(new PartnerSpotPromotedFromWaitingList
                {
                    Id = Guid.NewGuid(),
                    RegistrableId = spot.RegistrableId,
                    RegistrationId = spot.RegistrationId,
                    RegistrationId_Follower = spot.RegistrationId_Follower
                });
            }
            else
            {
                if (spot.RegistrationId.HasValue)
                {
                    _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                    {
                        Id = Guid.NewGuid(),
                        RegistrableId = spot.RegistrableId,
                        RegistrationId = spot.RegistrationId.Value
                    });
                }

                if (spot.RegistrationId_Follower.HasValue)
                {
                    _eventBus.Publish(new SingleSpotPromotedFromWaitingList
                    {
                        Id = Guid.NewGuid(),
                        RegistrableId = spot.RegistrableId,
                        RegistrationId = spot.RegistrationId_Follower.Value
                    });
                }
            }
        }
    }
}