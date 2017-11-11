using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.WaitingList
{
    public static class TryPromoteFromWaitingListHttpHandler
    {
        [FunctionName("TryPromoteFromWaitingListHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TryPromoteFromWaitingList/{eventIdString}")]HttpRequestMessage req, string eventIdString, TraceWriter log)
        {
            log.Info($"TryPromoteFromWaitingList for event {eventIdString}");

            var eventId = Guid.Parse(eventIdString);

            await ServiceBusClient.SendEvent(new TryPromoteFromWaitingListCommand { EventId = eventId }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);

            return req.CreateResponse(HttpStatusCode.OK, "Command is queued");
        }
    }
}