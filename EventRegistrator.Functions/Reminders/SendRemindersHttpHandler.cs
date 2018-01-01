using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Payments;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Reminders
{
    public static class SendRemindersHttpHandler
    {
        [FunctionName("SendRemindersHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "event/{eventIdString}/SendReminders")]
            HttpRequestMessage req, string eventIdString, TraceWriter log)
        {
            log.Info($"ProcessPaymentFilesCommand for event {eventIdString}");

            var eventId = Guid.Parse(eventIdString);

            await ServiceBusClient.SendEvent(new SendReminderCommand { EventId = eventId }, SendReminderCommandHandler.SendReminderCommandsQueueName);

            return req.CreateResponse(HttpStatusCode.OK, "Command is queued");
        }
    }
}
