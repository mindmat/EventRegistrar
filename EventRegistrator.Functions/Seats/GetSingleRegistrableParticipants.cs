using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Seats
{
    public static class GetSingleRegistrableParticipants
    {
        [FunctionName("GetSingleRegistrableParticipants")]
        public static async System.Threading.Tasks.Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "registrable/{registrableIdString}/GetSingleRegistrableParticipants")]
            HttpRequestMessage req,
            string registrableIdString,
            TraceWriter log)
        {
            if (!Guid.TryParse(registrableIdString, out var registrableId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"{registrableIdString} should be a GUID");
            }

            log.Info("C# HTTP trigger function processed a request.");

            using (var dbContext = new EventRegistratorDbContext())
            {
                var places = await dbContext.Seats
                                            .Where(seat => seat.RegistrableId == registrableId)
                                            .Include(seat => seat.Registration)
                                            .ToListAsync();

                return req.CreateResponse(HttpStatusCode.OK, places);
            }
        }
    }
}