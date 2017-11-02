using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
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

            using (var context = new EventRegistratorDbContext())
            {
                var registrablesToCheck = await context.Registrables.Where(rbl => rbl.EventId == command.EventId &&
                                                                                  (!command.RegistrableId.HasValue || rbl.Id == command.RegistrableId.Value))
                                                                    .Include(rbl => rbl.Seats)
                                                                    .ToListAsync();
                registrablesToCheck.ForEach(registrable => TryPromoteFromRegistrableWaitingList(registrable, context));
            }
        }

        private static void TryPromoteFromRegistrableWaitingList(Registrable registrable, EventRegistratorDbContext dbContext)
        {
            if (registrable.MaximumSingleSeats.HasValue)
            {
                var acceptedSeatCount = registrable.Seats.Count(seat => !seat.IsWaitingList);
                var seatsLeft = registrable.MaximumSingleSeats.Value - acceptedSeatCount;
                if (seatsLeft > 0)
                {
                    AcceptSingleSeatsFromWaitingList(registrable.Seats.Where(seat => seat.IsWaitingList).OrderBy(seat => seat.FirstPartnerJoined).Take(seatsLeft));
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
                    }
                }

                // try promote partner registration
                var acceptedSeatCount = registrable.Seats.Count(seat => !seat.IsWaitingList);
                var seatsLeft = registrable.MaximumDoubleSeats.Value - acceptedSeatCount;
                if (seatsLeft > 0)
                {
                    AcceptPartnerSeatsFromWaitingList(registrable.Seats.Where(seat => seat.IsWaitingList).OrderBy(seat => seat.FirstPartnerJoined).Take(seatsLeft));
                }
            }
        }

        private static void AcceptPartnerSeatsFromWaitingList(IEnumerable<Seat> take)
        {
        }

        private static void AcceptSingleSeatsFromWaitingList(IEnumerable<Seat> seatsToAccept)
        {
        }
    }
}