using System;
using System.Net;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions;

public static class ReceiveSendGridMailNotification
{
    [Function(nameof(ReceiveSendGridMailNotification))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mails/state")] HttpRequestData req,
                                                   ILogger log)
    {
        var bodyJson = await req.ReadAsStringAsync();
        log.LogInformation(bodyJson);

        var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                               .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        var id = Guid.NewGuid();
        await using (var connection = new SqlConnection(connectionString))
        {
            const string insertQuery = @"INSERT INTO dbo.RawMailEvents(Id, [Body], Created) " + @"VALUES (@Id, @Body, @Created)";
            var parameters = new
                             {
                                 Id = id,
                                 Body = bodyJson,
                                 Created = DateTimeOffset.Now
                             };

            await connection.ExecuteAsync(insertQuery, parameters);
        }

        await CommandQueue.SendCommand(new { RawMailEventsId = id });

        return req.CreateResponse(HttpStatusCode.OK);
    }
}