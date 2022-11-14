using System;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventRegistrar.Functions;

public static class TriggerBankStatementsImport
{
    [Function(nameof(TriggerBankStatementsImport))]
    public static async Task Run([TimerTrigger("0 0 3 * * 1-6")] TimerInfo _)
    {
        var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                               .Build();
        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        var eventIds = (await connection.QueryAsync<Guid>(@"
                   SELECT CFG.EventId
                   FROM dbo.EventConfiguration CFG
                     INNER JOIN dbo.[Events]   EVT ON EVT.Id = CFG.EventId
                   WHERE CFG.[Type] = 'EventRegistrar.Backend.Payments.Files.Fetch.FetchBankStatementsFilesConfiguration'
                     AND EVT.[State] IN (1,2)")).AsList();

        foreach (var eventId in eventIds)
        {
            await CommandQueue.SendCommand("EventRegistrar.Backend.Payments.Files.Fetch.FetchBankStatementsFileCommand", new { EventId = eventId });
        }
    }
}