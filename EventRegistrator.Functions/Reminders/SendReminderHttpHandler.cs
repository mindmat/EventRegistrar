using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Reminders
{
    public static class SendReminderHttpHandler
    {
        [FunctionName("SendReminderHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registration/{registrationIdString:guid}/sendReminder")]
            HttpRequestMessage req,
            string registrationIdString,
            TraceWriter log)
        {
            var registrationId = Guid.Parse(registrationIdString);
            var withhold = req.GetQueryNameValuePairs().FirstOrDefault(kvp => string.Compare(kvp.Key, "withhold", StringComparison.OrdinalIgnoreCase) == 0).Value == "true";

            await ServiceBusClient.SendEvent(new SendReminderCommand { RegistrationId = registrationId, Withhold = withhold }, SendReminderCommandHandler.SendReminderCommandsQueueName);

            return req.CreateResponse(HttpStatusCode.OK, "Command is queued");
        }
    }
}