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

            if (registrable.MaximumSingleSeats.HasValue)
            {
                var acceptedSeatCount = registrable.Seats.Count(seat => !seat.IsWaitingList);
                var seatsLeft = registrable.MaximumSingleSeats.Value - acceptedSeatCount;
                if (seatsLeft > 0)
                {
                    return await AcceptSingleSeatsFromWaitingList(registrable.Seats.Where(seat => seat.IsWaitingList).OrderBy(seat => seat.FirstPartnerJoined).Take(seatsLeft), dbContext, log);
                }
            }
            else if (registrable.MaximumDoubleSeats.HasValue)
            {
                // try match single leaders and followers
                var singleSeats = registrable.Seats.Where(seat => seat.PartnerEmail == null).ToList();
                var acceptedSingleLeaders = singleSeats.Where(seat => !seat.IsWaitingList &&
                                                                      !seat.RegistrationId_Follower.HasValue)
                                                       .ToList();
                var acceptedSingleFollowers = singleSeats.Where(seat => !seat.IsWaitingList &&
                                                                        !seat.RegistrationId.HasValue)
                                                         .ToList();

                var waitingFollowers = new Queue<Seat>(registrable.Seats.Where(seat => seat.IsWaitingList &&
                                                                                       !seat.RegistrationId.HasValue)
                                                                        .OrderBy(seat => seat.FirstPartnerJoined));
                var waitingLeaders = new Queue<Seat>(registrable.Seats.Where(seat => seat.IsWaitingList &&
                                                                                     !seat.RegistrationId_Follower.HasValue)
                                                                      .OrderBy(seat => seat.FirstPartnerJoined));
                log.Info($"Registrable {registrable.Name}, leaders in {acceptedSingleLeaders.Count}, followers in {acceptedSingleFollowers.Count}, leaders waiting {waitingLeaders.Count}, followers waiting {waitingFollowers.Count}");
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
                    }
                }

                // try promote partner registration
                // ToDo: precedence partner vs. single registrations
                var acceptedSeatCount = registrable.Seats.Count(seat => !seat.IsWaitingList);
                var seatsLeft = registrable.MaximumDoubleSeats.Value - acceptedSeatCount;
                if (seatsLeft > 0)
                {
                    return AcceptPartnerSeatsFromWaitingList(registrable.Seats.Where(seat => seat.IsWaitingList).OrderBy(seat => seat.FirstPartnerJoined).Take(seatsLeft));
                }
            }
            return registrationIdsToCheck;
        }

        private static IEnumerable<Guid?> AcceptPartnerSeatsFromWaitingList(IEnumerable<Seat> seatsToAccept)
        {
            foreach (var seat in seatsToAccept)
            {
                seat.IsWaitingList = false;
                if (seat.RegistrationId.HasValue)
                {
                    yield return seat.RegistrationId;
                }
                if (seat.RegistrationId_Follower.HasValue)
                {
                    yield return seat.RegistrationId_Follower;
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