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

namespace EventRegistrator.Functions.Mailing
{
    public static class GetRegistrationMails
    {
        [FunctionName("GetRegistrationMails")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrations/{registrationIdString:guid}/mails")]
            HttpRequestMessage req,
            string registrationIdString,
            TraceWriter log)
        {
            if (!Guid.TryParse(registrationIdString, out var registrationId))
            {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, $"{registrationIdString} is not a guid");
            }

            using (var dbContext = new EventRegistratorDbContext())
            {
                var mails = await dbContext.Mails
                                           .Where(mail => mail.Registrations.Any(reg => reg.RegistrationId == registrationId))
                                           .OrderByDescending(mail => mail.Created)
                                           .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, mails);
            }
        }
    }
}
