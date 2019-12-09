using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace EventRegistrar.Functions
{
    public static class TriggerBankStatementsImport
    {
        [FunctionName("TriggerBankStatementsImport")]
        public static async Task Run([TimerTrigger("0 0 3 * * 1-6")]TimerInfo timer, ILogger log)
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                       .Build();
            var connectionString = config.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                var eventIds = (await connection.QueryAsync<Guid>(@"
                   SELECT CFG.EventId
                   FROM dbo.EventConfiguration CFG
                     INNER JOIN dbo.[Events]   EVT ON EVT.Id = CFG.EventId
                   WHERE CFG.[Type] = 'EventRegistrar.Backend.Payments.Files.Fetch.FetchBankStatementsFilesConfiguration'
                     AND EVT.[State] IN (1,2)")).AsList();
                log.LogInformation("{0} events will be triggered", eventIds.Count);
                foreach (var eventId in eventIds)
                {
                    var command = new { EventId = eventId };
                    var message = new
                    {
                        CommandType = "EventRegistrar.Backend.Payments.Files.Fetch.FetchBankStamentsFileCommand",
                        CommandSerialized = JsonConvert.SerializeObject(command)
                    };
                    await ServiceBusClient.SendCommand(message, "CommandQueue");
                }
            }
        }
    }
}
