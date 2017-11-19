using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
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
                var registrationsToCheckMail = new List<Guid>();
                var registrables = await dbContext.Registrables
                                                  .Where(rbl => rbl.EventId == eventId && rbl.MaximumDoubleSeats.HasValue)
                                                  .ToListAsync();

                var registrations = (await dbContext.Registrations
                                                   .Where(reg => reg.RegistrationForm.EventId == eventId)
                                                   .ToDictionaryAsync(reg => reg.RespondentEmail, reg => reg.Id))
                                    .ToDictionary(tmp => tmp.Key?.ToLowerInvariant(), tmp => tmp.Value);
                foreach (var registrable in registrables)
                {
                    registrationsToCheckMail.AddRange(await MergeDuplicatePartnerSeatsInRegistrable(registrable.Id, registrations, dbContext, log));
                }
                await dbContext.SaveChangesAsync();

                foreach (var registrationIdToCheckMail in registrationsToCheckMail)
                {
                    await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registrationIdToCheckMail }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
                }
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static async Task<IEnumerable<Guid>> MergeDuplicatePartnerSeatsInRegistrable(Guid registrableId, Dictionary<string, Guid> registrations, EventRegistratorDbContext dbContext, TraceWriter log)
        {
            var unmatchedPartnerSeats = await dbContext.Seats
                                                       .Where(seat => seat.RegistrableId == registrableId &&
                                                                      !seat.IsCancelled &&
                                                                      seat.PartnerEmail != null &&
                                                                      (seat.RegistrationId == null || seat.RegistrationId_Follower == null))
                                                       .ToListAsync();
            log.Info($"Merge for registrable {registrableId}, registration lookup count {registrations.Count}, unmatchedPartnerSeats {unmatchedPartnerSeats.Count}");

            var registrationsToCheckMail = new List<Guid>();

            foreach (var unmatchedLeaderSeat in unmatchedPartnerSeats.Where(seat => seat.RegistrationId_Follower == null && seat.RegistrationId.HasValue))
            {
                if (!unmatchedLeaderSeat.RegistrationId.HasValue || !registrations.ContainsValue(unmatchedLeaderSeat.RegistrationId.Value))
                {
                    continue;
                }
                var leaderEmail = registrations.First(kvp => kvp.Value == unmatchedLeaderSeat.RegistrationId.Value).Key?.ToLowerInvariant().Trim();
                if (!registrations.TryGetValue(unmatchedLeaderSeat.PartnerEmail, out var followerRegistrationId))
                {
                    continue;
                }
                log.Info($"followerRegistrationId {followerRegistrationId}");

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

                unmatchedLeaderSeat.RegistrationId_Follower = unmatchedFollowerSeat.RegistrationId_Follower;
                unmatchedLeaderSeat.IsWaitingList = mergedSeatIsWaitingList;
                unmatchedLeaderSeat.FirstPartnerJoined = mergedFirstPartnerJoined;

                dbContext.Seats.Remove(unmatchedFollowerSeat);

                registrationsToCheckMail.Add(unmatchedLeaderSeat.RegistrationId.Value);
            }
            return registrationsToCheckMail;
        }
    }
}