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
    public static class GetRegistrationInfosForCheckin
    {
        [FunctionName("GetRegistrationInfosForCheckin")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString:guid}/checkinView")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrations = await dbContext.Registrations
                                                   .Where(reg => reg.RegistrationForm.EventId == eventId &&
                                                                 reg.IsWaitingList == false &&
                                                                 reg.State != RegistrationState.Cancelled)
                                                   .Select(reg => new
                                                   {
                                                       reg.Id,
                                                       Email = reg.RespondentEmail,
                                                       FirstName = reg.RespondentFirstName,
                                                       LastName = reg.RespondentLastName,
                                                       Kurs = reg.Seats_AsLeader.Where(seat => seat.Registrable.CheckinListColumn == "Kurs" && !seat.IsCancelled)
                                                                                .Select(seat => seat.Registrable.Name)
                                                                                .FirstOrDefault() ??
                                                              reg.Seats_AsFollower.Where(seat => seat.Registrable.CheckinListColumn == "Kurs" && !seat.IsCancelled)
                                                                                  .Select(seat => seat.Registrable.Name)
                                                                                  .FirstOrDefault(),
                                                       MittagessenSamstag = reg.Seats_AsLeader.Where(seat => seat.Registrable.CheckinListColumn == "Mittagessen Samstag" && !seat.IsCancelled)
                                                                                              .Select(seat => seat.Registrable.Name)
                                                                                              .FirstOrDefault(),
                                                       MittagessenSonntag = reg.Seats_AsLeader.Where(seat => seat.Registrable.CheckinListColumn == "Mittagessen Sonntag" && !seat.IsCancelled)
                                                                                              .Select(seat => seat.Registrable.Name)
                                                                                              .FirstOrDefault(),
                                                       PartyPass = reg.Seats_AsLeader.Count(seat => seat.Registrable.CheckinListColumn == "Parties" && !seat.IsCancelled) == 3,
                                                       Status = reg.State.ToString()
                                                   })
                                                   .Where(reg => reg.PartyPass || reg.Kurs != null)
                                                   .OrderBy(reg => reg.Kurs)
                                                   .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, registrations);
            }
        }
    }
}
