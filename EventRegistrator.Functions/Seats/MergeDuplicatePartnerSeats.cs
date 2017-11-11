using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Seats
{
    public static class MergeDuplicatePartnerSeats
    {
        [FunctionName("MergeDuplicatePartnerSeats")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event/{eventIdString}/MergeDuplicatePartnerSeats")]
                   HttpRequestMessage req,
                   string eventIdString,
                   TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrables = await dbContext.Registrables
                                                  .Where(rbl => rbl.EventId == eventId && rbl.MaximumDoubleSeats.HasValue)
                                                  .Include(rbl => rbl.Seats)
                                                  .ToListAsync();

                var registrations = await dbContext.Registrations
                                                   .Where(reg => reg.RegistrationForm.EventId == eventId)
                                                   .ToDictionaryAsync(reg => reg.RespondentEmail, reg => reg.Id);
                foreach (var registrable in registrables)
                {
                    await MergeDuplicatePartnerSeatsInRegistrable(registrable.Id, registrations, dbContext, log);
                }
                //await dbContext.SaveChangesAsync();
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static async Task MergeDuplicatePartnerSeatsInRegistrable(Guid registrableId, Dictionary<string, Guid> registrations, EventRegistratorDbContext dbContext, TraceWriter log)
        {
            var unmatchedPartnerSeats = await dbContext.Seats
                                                       .Where(seat => seat.RegistrableId == registrableId &&
                                                                      seat.PartnerEmail != null &&
                                                                      (seat.RegistrationId == null || seat.RegistrationId_Follower == null))
                                                       .ToListAsync();
            foreach (var unmatchedLeaderSeat in unmatchedPartnerSeats.Where(seat => seat.RegistrationId_Follower == null))
            {
                if (!registrations.ContainsValue(unmatchedLeaderSeat.Id))
                {
                    continue;
                }
                var leaderEmail = registrations.First(kvp => kvp.Value == unmatchedLeaderSeat.Id).Key;
                if (!registrations.TryGetValue(unmatchedLeaderSeat.PartnerEmail, out var followerRegistrationId))
                {
                    continue;
                }
                var unmatchedFollowerSeat = unmatchedPartnerSeats
                                            .FirstOrDefault(seat => seat.RegistrationId_Follower == followerRegistrationId &&
                                                                    seat.PartnerEmail == leaderEmail);
                if (unmatchedFollowerSeat == null)
                {
                    continue;
                }
                var mergedSeatIsWaitingList = unmatchedLeaderSeat.IsWaitingList && unmatchedFollowerSeat.IsWaitingList;
                var mergedFirstPartnerJoined = unmatchedLeaderSeat.FirstPartnerJoined < unmatchedFollowerSeat.FirstPartnerJoined ? unmatchedLeaderSeat.FirstPartnerJoined : unmatchedFollowerSeat.FirstPartnerJoined;
                log.Info($"duplicate registration detected: leader seat {unmatchedLeaderSeat.Id}, follower seat {unmatchedFollowerSeat.Id}, Waiting List {mergedSeatIsWaitingList}, FirstPartnerJoined {mergedFirstPartnerJoined}");

                //unmatchedLeaderSeat.RegistrationId_Follower = unmatchedFollowerSeat.RegistrationId_Follower;
                //unmatchedLeaderSeat.IsWaitingList = mergedSeatIsWaitingList;
                //unmatchedLeaderSeat.FirstPartnerJoined = mergedFirstPartnerJoined;

                //dbContext.Seats.Remove(unmatchedFollowerSeat);
            }
        }
    }
}