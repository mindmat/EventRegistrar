using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Mailing
{
    public static class ComposeAndSendMailHttpHandler
    {
        [FunctionName("ComposeAndSendMailHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrations/{registrationIdString}/composeAndSendMail")]
               HttpRequestMessage req, string registrationIdString, TraceWriter log)
        {
            var registrationId = Guid.Parse(registrationIdString);
            var withhold = req.GetQueryNameValuePairs().FirstOrDefault(kvp => string.Compare(kvp.Key, "withhold", StringComparison.OrdinalIgnoreCase) == 0).Value == "true";
            var allowDuplicate = req.GetQueryNameValuePairs().FirstOrDefault(kvp => string.Compare(kvp.Key, "allowDuplicate", StringComparison.OrdinalIgnoreCase) == 0).Value == "true";
            await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registrationId, Withhold = withhold, AllowDuplicate = allowDuplicate }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
            return req.CreateResponse(HttpStatusCode.OK, $"command has been queried (withhold = {withhold})");
        }
    }
}