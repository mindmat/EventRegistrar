using System;
using System.Threading.Tasks;

using Dapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions
{
    public static class SetSmsStatus
    {
        [FunctionName("SetSmsStatus")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventIdString}/sms/setStatus")]
                                                     HttpRequest req,
                                                     ILogger log,
                                                     string eventIdString)
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                                   .Build();
            var connectionString = config.GetConnectionString("DefaultConnection");

            var smsSid = req.Form["SmsSid"];
            var messageStatus = req.Form["MessageStatus"];
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
                log.LogInformation("{0} rows updated", affectedRows);
            }
            return new OkResult();
        }
    }
}