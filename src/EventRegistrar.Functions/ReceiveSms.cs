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

        var command = new { Sms = twilioSms };
        var message = new
                      {
                          CommandType = "EventRegistrar.Backend.PhoneMessages.ProcessReceivedSmsCommand",
                          CommandSerialized = JsonSerializer.Serialize(command)
                      };
        await CommandQueue.SendCommand(message);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}