using EventRegistrator.Functions.Events;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Registrables
{
    public static class DoubleRegistrableOverview
    {
        [FunctionName("DoubleRegistrableOverview")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.User, "get", Route = "{eventAcronym}/doubleRegistrableOverview")]
            HttpRequestMessage req,
            TraceWriter log,
            string eventAcronym)
        {
            log.Info(string.Join(Environment.NewLine, ClaimsPrincipal.Current.Claims.Select(clm => $"Claim: {clm.Type} = {clm.Value} (Issuer {clm.Issuer})")));

            using (var dbContext = new EventRegistratorDbContext())
            {
                var eventId = await dbContext.Events.GetEventId(eventAcronym);
                log.Info($"Event: {eventAcronym} -> Id: {eventId}");

                var registrables = await dbContext.Registrables
                                                  .Where(rbl => rbl.EventId == eventId
                                                             && rbl.MaximumDoubleSeats.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Include(rbl => rbl.Seats)
                                                  .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, registrables.Select(rbl => new
                {
                    rbl.Id,
                    rbl.Name,
                    SpotsAvailable = rbl.MaximumDoubleSeats,
                    LeadersAccepted = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                              seat.RegistrationId.HasValue &&
                                                              !seat.IsWaitingList),
                    FollowersAccepted = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                                seat.RegistrationId_Follower.HasValue &&
                                                                !seat.IsWaitingList),
                    LeadersOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                                   seat.RegistrationId.HasValue &&
                                                                   seat.IsWaitingList &&
                                                                   seat.PartnerEmail == null),
                    FollowersOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                                     seat.RegistrationId_Follower.HasValue &&
                                                                     seat.IsWaitingList &&
                                                                     seat.PartnerEmail == null),
                    CouplesOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                                   seat.RegistrationId_Follower.HasValue &&
                                                                   seat.IsWaitingList &&
                                                                   seat.PartnerEmail != null)
                }));
            }
        }
    }
}