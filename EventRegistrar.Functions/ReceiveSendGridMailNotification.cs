using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions
{
    public static class ReceiveSendGridMailNotification
    {
        [FunctionName("ReceiveSendGridMailNotification")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mails/state")]
                                                    HttpRequest req,
                                                    ILogger log)
        {
            var bodyJson = await req.ReadAsStringAsync();
            log.LogInformation(bodyJson);

            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            var id = Guid.NewGuid();
            using (var connection = new SqlConnection(connectionString))
            {
                const string insertQuery = @"INSERT INTO dbo.RawMailEvents(Id, [Body], Created) " +
                                           @"VALUES (@Id, @Body, @Created)";
                var parameters = new
                {
                    Id = id,
                    Body = bodyJson,
                    Created = DateTime.UtcNow
                };

                await connection.ExecuteAsync(insertQuery, parameters);
            }

            await ServiceBusClient.SendCommand(new { RawMailEventsId = id }, "processmailevents");

            return new OkResult();
        }
    }
}