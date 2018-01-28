using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Reminders
{
    public static class GetDuePayments
    {
        [FunctionName("GetDuePayments")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString}/duePayments")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var dueRegistrations = await dbContext.Registrations
                                                      .Where(reg => reg.RegistrationForm.EventId == eventId &&
                                                                    reg.State == RegistrationState.Received &&
                                                                    reg.IsWaitingList != true)
                                                      .Select(reg => new
                                                      {
                                                          reg.Id,
                                                          FirstName = reg.RespondentFirstName,
                                                          LastName = reg.RespondentLastName,
                                                          reg.Price,
                                                          reg.ReceivedAt,
                                                          reg.ReminderLevel,
                                                          Paid = (decimal?)reg.Payments.Sum(ass => ass.Amount)
                                                      })
                                                      .OrderBy(reg => reg.ReceivedAt)
                                                      .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, dueRegistrations);
            }
        }
    }
}
