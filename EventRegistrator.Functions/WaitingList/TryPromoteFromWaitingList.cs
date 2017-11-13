using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using EventRegistrator.Functions.Registrables;
using EventRegistrator.Functions.Seats;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EventRegistrator.Functions.WaitingList
{
    public static class TryPromoteFromWaitingList
    {
        public const string TryPromoteFromWaitingListQueueName = "TryPromoteFromWaitingList";

        [FunctionName("TryPromoteFromWaitingList")]
        public static async Task Run([ServiceBusTrigger(TryPromoteFromWaitingListQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]TryPromoteFromWaitingListCommand command, TraceWriter log)
        {
            log.Info($"TryPromoteFromWaitingList triggered: {command.EventId}, {command.RegistrableId}");

            var registrationIdsToCheck = new List<Guid?>();
            using (var context = new EventRegistratorDbContext())
            {
                var registrablesToCheck = await context.Registrables.Where(rbl => rbl.EventId == command.EventId &&
                                                                                  (!command.RegistrableId.HasValue || rbl.Id == command.RegistrableId.Value))
                                                                    .Include(rbl => rbl.Seats)
                                                                    .ToListAsync();
                foreach (var registrable in registrablesToCheck)
                {
                    registrationIdsToCheck.AddRange(await TryPromoteFromRegistrableWaitingList(registrable, context, log));
                }
                await context.SaveChangesAsync();
            }

            foreach (var registrationId in registrationIdsToCheck.Where(id => id.HasValue))
            {
                await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registrationId }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
            }
        }

        private static async Task<IEnumerable<Guid?>> TryPromoteFromRegistrableWaitingList(Registrable registrable, EventRegistratorDbContext dbContext, TraceWriter log)
        {
            var registrationIdsToCheck = new List<Guid?>();
            var seats = registrable.Seats.Where(seat => !seat.IsCancelled).ToList();
            if (registrable.MaximumSingleSeats.HasValue)
            {
                var acceptedSeatCount = seats.Count(seat => !seat.IsWaitingList);
                var seatsLeft = registrable.MaximumSingleSeats.Value - acceptedSeatCount;
                if (seatsLeft > 0)
                {
                    return await AcceptSingleSeatsFromWaitingList(seats.Where(seat => seat.IsWaitingList).OrderBy(seat => seat.FirstPartnerJoined).Take(seatsLeft), dbContext, log);
                }
            }
            else if (registrable.MaximumDoubleSeats.HasValue)
            {
                // try match single leaders and followers
                var singleSeats = seats.Where(seat => seat.PartnerEmail == null).ToList();
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
                log.Info($"Registrable {registrable.Name}, single leaders in {acceptedSingleLeaders.Count}, single followers in {acceptedSingleFollowers.Count}, single leaders waiting {waitingLeaders.Count}, single followers waiting {waitingFollowers.Count}");
                var singleRegistrationsPromoted = false;
                if (acceptedSingleLeaders.Any() && waitingFollowers.Any())
                {
                    foreach (var acceptedSingleLeader in acceptedSingleLeaders)
                    {
                        if (waitingFollowers.Peek() == null)
                        {
                            // no more waiting followers
                            break;
                        }
                        var waitingFollower = waitingFollowers.Dequeue();
                        acceptedSingleLeader.RegistrationId_Follower = waitingFollower.RegistrationId_Follower;
                        dbContext.Seats.Remove(waitingFollower);
                        registrationIdsToCheck.Add(waitingFollower.RegistrationId_Follower);
                        await SetRegistrationNotOnWaitingListAnymore(waitingFollower.RegistrationId_Follower, dbContext, log);
                        singleRegistrationsPromoted = true;
                    }
                }

                if (acceptedSingleFollowers.Any() && waitingLeaders.Any())
                {
                    foreach (var acceptedSingleFollower in acceptedSingleFollowers)
                    {
                        if (waitingLeaders.Peek() == null)
                        {
                            // no more waiting followers
                            break;
                        }
                        var waitingLeader = waitingLeaders.Dequeue();
                        acceptedSingleFollower.RegistrationId_Follower = waitingLeader.RegistrationId;
                        dbContext.Seats.Remove(waitingLeader);
                        registrationIdsToCheck.Add(waitingLeader.RegistrationId_Follower);
                        await SetRegistrationNotOnWaitingListAnymore(waitingLeader.RegistrationId, dbContext, log);
                        singleRegistrationsPromoted = true;
                    }
                }

                // be defenisve - registrable.Seats might be inconsistent with the actual seats on the db
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

        private static IEnumerable<Guid?> AcceptPartnerSeatsFromWaitingList(Registrable registrable, int seatsLeft)
        {
            var seatsToAccept = registrable.Seats
                                           .Where(seat => seat.IsWaitingList && !seat.IsCancelled)
                                           .OrderBy(seat => seat.FirstPartnerJoined);
            foreach (var seat in seatsToAccept)
            {
                if (seat.PartnerEmail == null)
                {
                    // single registration, check imbalance
                    var ownRole = seat.RegistrationId.HasValue ? Role.Leader : Role.Follower;
                    if (!ImbalanceManager.CanAddNewDoubleSeatForSingleRegistration(registrable, ownRole))
                    {
                        yield break;
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

        private static async Task<IEnumerable<Guid?>> AcceptSingleSeatsFromWaitingList(IEnumerable<Seat> seatsToAccept, EventRegistratorDbContext dbContext, TraceWriter log)
        {
            var registrationIdsToCheck = new List<Guid?>();
            foreach (var seat in seatsToAccept)
            {
                seat.IsWaitingList = false;
                await SetRegistrationNotOnWaitingListAnymore(seat.RegistrationId, dbContext, log);
                registrationIdsToCheck.Add(seat.RegistrationId);
            }
            return registrationIdsToCheck;
        }

        private static async Task SetRegistrationNotOnWaitingListAnymore(Guid? registrationId, EventRegistratorDbContext dbContext, TraceWriter log)
        {
            var registration = await dbContext.Registrations.FirstOrDefaultAsync(reg => registrationId.HasValue && reg.Id == registrationId);

            if (registration == null)
            {
                log.Info($"No registration found with id {registrationId}");
            }
            else
            {
                registration.IsWaitingList = false;
            }
        }
    }
}