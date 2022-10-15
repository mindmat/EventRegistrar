using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;

using Dapper;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions;

public static class SetSmsStatus
{
    [Function(nameof(SetSmsStatus))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventIdString}/sms/setStatus")] HttpRequestData req,
                                                   string eventIdString)
    {
        var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                               .Build();
        var connectionString = config.GetConnectionString("DefaultConnection");

        var stringBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Parse as query string
        var keyValues = HttpUtility.ParseQueryString(stringBody);

        var smsSid = keyValues["SmsSid"];
        var messageStatus = keyValues["MessageStatus"];
        var eventId = Guid.Parse(eventIdString);

        await using (var connection = new SqlConnection(connectionString))
        {
            var affectedRows = await connection.ExecuteAsync(@"
                   UPDATE SMS
                     SET SmsStatus = @Status
                   FROM dbo.Sms SMS
                     INNER JOIN dbo.Registrations REG ON REG.Id = SMS.RegistrationId
                   WHERE REG.EventID = @EventId
                     AND SMS.SmsSid = @Sid", new { Sid = smsSid, EventId = eventId, Status = messageStatus });
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}