using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

            var registrations = await CheckinDataView.GetCheckinData(eventId);
            return req.CreateResponse(HttpStatusCode.OK, registrations);
        }
    }
}
