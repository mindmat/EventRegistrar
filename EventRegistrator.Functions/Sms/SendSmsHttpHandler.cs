using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Sms
{
    public static class SendSmsHttpHandler
    {
        [FunctionName("SendSmsHttpHandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrations/{registrationIdString:guid}/sendSms")]
            HttpRequestMessage req,
            string registrationIdString,
            TraceWriter log)
        {
            var registrationId = Guid.Parse(registrationIdString);
            var body = await req.Content.ReadAsStringAsync();

            log.Info(body);
            return req.CreateResponse(HttpStatusCode.OK);
            //return await SmsSender.SendSms(registrationId, body, req, log);
        }
    }
}
