using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Registrations
{
    public static class GetPartyOverview
    {
        [FunctionName("GetPartyOverview")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString:guid}/partyOverview")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var mappings = await dbContext.RegistrableCompositions
                                              .Where(cmp => cmp.Registrable.EventId == eventId)
                                              .GroupBy(cmp => cmp.RegistrableId_Contains)
                                              .Select(grp => new
                                              {
                                                  PartyId = grp.Key,
                                                  DependentRegistrablesIds = grp.Select(rbl => rbl.RegistrableId).ToList()
                                              })
                                              .ToListAsync();

                var idsOfInterest = mappings.Select(map => map.PartyId).ToList();
                idsOfInterest.AddRange(mappings.SelectMany(map => map.DependentRegistrablesIds));

                log.Info(string.Join(",", idsOfInterest));

                var participants = await dbContext.Registrables
                                                  .Where(rbl => idsOfInterest.Contains(rbl.Id))
                                                  .Select(rbl => new
                                                  {
                                                      rbl.Id,
                                                      rbl.Name,
                                                      rbl.ShowInMailListOrder,
                                                      Participants = rbl.Seats.Where(seat => !seat.IsCancelled && !seat.IsWaitingList)
                                                                              .Select(seat => (seat.RegistrationId.HasValue ? 1 : 0) +
                                                                                              (seat.RegistrationId_Follower.HasValue ? 1 : 0))
                                                                              .Sum(),
                                                      Potential = rbl.MaximumSingleSeats ?? (rbl.MaximumDoubleSeats ?? 0) * 2
                                                  })
                                                  .ToDictionaryAsync(rbl => rbl.Id);

                var overview = mappings.Select(map => new
                {
                    map.PartyId,
                    participants[map.PartyId].Name,
                    participants[map.PartyId].ShowInMailListOrder,
                    Direct = participants[map.PartyId].Participants,
                    Total = participants[map.PartyId].Participants + participants.Values.Where(rbl => map.DependentRegistrablesIds.Contains(rbl.Id))
                                                                                        .Sum(rbl => rbl.Participants),
                    Potential = participants.Values.Where(rbl => map.DependentRegistrablesIds.Contains(rbl.Id))
                                                   .Sum(rbl => Math.Max(0, rbl.Potential - rbl.Participants)),
                    Details = participants.Values.Where(rbl => map.DependentRegistrablesIds.Contains(rbl.Id))
                                                 .Select(rbl => new
                                                 {
                                                     rbl.Id,
                                                     rbl.Name,
                                                     rbl.Participants,
                                                     Potential = rbl.Potential - rbl.Participants
                                                 })

                })
                .OrderBy(map => map.ShowInMailListOrder);

                return req.CreateResponse(HttpStatusCode.OK, overview);
            }
        }
    }
}
