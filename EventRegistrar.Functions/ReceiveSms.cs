using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventRegistrar.Functions
{
    public static class ReceiveSms
    {
        [FunctionName("ReceiveSms")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sms/reply")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation(JsonConvert.SerializeObject(req.Form));
            var twilioSms = req.Form.Deserialize<TwilioSms>();

            await ServiceBusClient.SendCommand(new { Sms = twilioSms }, "processreceivedsms");
            return new OkResult();
        }
    }
}