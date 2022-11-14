using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions;

public static class ReceiveSms
{
    [Function(nameof(ReceiveSms))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sms/reply")] HttpRequestData req,
                                                   ILogger log)
    {
        // Read body
        var stringBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Parse as query string
        var keyValues = HttpUtility.ParseQueryString(stringBody);

        var twilioSms = keyValues.Deserialize<TwilioSms>();

        await CommandQueue.SendCommand("EventRegistrar.Backend.PhoneMessages.ProcessReceivedSmsCommand", new { Sms = twilioSms });

        return req.CreateResponse(HttpStatusCode.OK);
    }
}