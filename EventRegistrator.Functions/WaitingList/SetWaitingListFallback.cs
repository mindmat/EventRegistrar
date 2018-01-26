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

namespace EventRegistrator.Functions.WaitingList
{
    public static class SetWaitingListFallback
    {
        [FunctionName("SetWaitingListFallback")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrations/{registrationIdString}/SetWaitingListFallback")]
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
                var registration = await dbContext.Registrations.FirstOrDefaultAsync(reg => reg.Id == registrationId);
                if (registration == null)
                {
                    return req.CreateErrorResponse(HttpStatusCode.NotFound, $"No registration with id {registrationId} found");
                }
                registration.FallbackToPartyPass = true;
                await dbContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
