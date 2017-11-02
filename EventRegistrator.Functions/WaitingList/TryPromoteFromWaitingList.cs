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
                //var acceptedSeats = registrable.Seats.Where(seat => !seat.IsWaitingList).ToList();
                var acceptedExtraLeaders = singleSeats.Where(seat => !seat.IsWaitingList &&
                                                                     !seat.RegistrationId_Follower.HasValue);
                var acceptedExtraFollowers = singleSeats.Where(seat => !seat.IsWaitingList &&
                                                                       !seat.RegistrationId.HasValue);
                var waitingFollowers = new Queue<Seat>(registrable.Seats.Where(seat => seat.IsWaitingList &&
                                                                                       !seat.RegistrationId.HasValue)
                                                                        .OrderBy(seat => seat.FirstPartnerJoined));
                var waitingLeaders = new Queue<Seat>(registrable.Seats.Where(seat => seat.IsWaitingList &&
                                                                                     !seat.RegistrationId_Follower.HasValue)
                                                                      .OrderBy(seat => seat.FirstPartnerJoined));

                if (acceptedExtraLeaders.Any() && waitingFollowers.Any())
                {
                    foreach (var acceptedExtraLeader in acceptedExtraLeaders)
                    {
                        if (waitingFollowers.Peek() == null)
                        {
                            // no more waiting followers
                            break;
                        }
                        var waitingFollower = waitingFollowers.Dequeue();
                        acceptedExtraLeader.RegistrationId_Follower = waitingFollower.RegistrationId_Follower;
                        dbContext.Seats.Remove(waitingFollower);
                    }
                }

                var leadersOnWaitingList = registrable.Seats
                    .Where(seat => seat.IsWaitingList && seat.RegistrationId.HasValue)
                    .OrderBy(seat => seat.FirstPartnerJoined);

                // try promote partner registration
            }
        }

        private static void AcceptSingleSeatsFromWaitingList(IEnumerable<Seat> seatsToAccept)
        {
            throw new System.NotImplementedException();
        }
    }
}