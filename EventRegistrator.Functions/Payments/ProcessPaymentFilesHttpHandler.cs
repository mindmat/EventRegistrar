using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Payments
{
    public static class ProcessPaymentFilesHttpHandler
    {
        [FunctionName("ProcessPaymentFilesHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event/{eventIdString}/ProcessPaymentFiles")]HttpRequestMessage req, string eventIdString, TraceWriter log)
        {
            log.Info($"ProcessPaymentFilesCommand for event {eventIdString}");

            var eventId = Guid.Parse(eventIdString);

            await ServiceBusClient.SendEvent(new ProcessPaymentFilesCommand { EventId = eventId }, ProcessPaymentFiles.ProcessPaymentFilesQueueName);

            return req.CreateResponse(HttpStatusCode.OK, "Command is queued");
        }
    }
}