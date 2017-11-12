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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registration/{registrationIdString}/ComposeAndSendMail")]
               HttpRequestMessage req, string registrationIdString, TraceWriter log)
        {
            var registrationId = Guid.Parse(registrationIdString);
            await ServiceBusClient.SendEvent(new ComposeAndSendMailCommand { RegistrationId = registrationId }, ComposeAndSendMailCommandHandler.ComposeAndSendMailCommandsQueueName);
            return req.CreateResponse(HttpStatusCode.OK, "command has been queried");
        }
    }
}