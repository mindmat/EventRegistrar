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

namespace EventRegistrator.Functions.Sms
{
    public static class GetSmsConversation
    {
        [FunctionName("GetSmsConversation")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrations/{registrationIdString:guid}/smsConversation")]
            HttpRequestMessage req,
            string registrationIdString,
            TraceWriter log)
        {
            var registrationId = Guid.Parse(registrationIdString);

            using (var dbContext = new EventRegistratorDbContext())
            {
                var sms = await dbContext.Sms
                                         .Where(s => s.RegistrationId == registrationId)
                                         .Select(s => new
                                         {
                                             Status = s.SmsStatus,
                                             s.Body,
                                             Sent = s.Sent.HasValue,
                                             Date = s.Sent ?? s.Received
                                         })
                                         .OrderBy(s => s.Date)
                                         .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, sms);
            }
        }
    }
}
