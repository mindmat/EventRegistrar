using System;
using System.Net;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventRegistrar.Functions;

public static class ReceivePostmarkMailNotification
{
    [Function(nameof(ReceivePostmarkMailNotification))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mails/state/postmark/{type:alpha?}")] HttpRequestData req, string? type)
    {
        var bodyJson = await req.ReadAsStringAsync();

        var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                               .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        var id = Guid.NewGuid();
        await using (var connection = new SqlConnection(connectionString))
        {
            const string insertQuery = "INSERT INTO dbo.RawMailEvents(Id, MailSender, Type, [Body], Created) VALUES (@Id, 3, @Type, @Body, @Created)";
            var parameters = new
                             {
                                 Id = id,
                                 Type = type,
                                 Body = bodyJson,
                                 Created = DateTimeOffset.Now
                             };

            await connection.ExecuteAsync(insertQuery, parameters);
        }

        await CommandQueue.SendCommand("EventRegistrar.Backend.Mailing.Feedback.ProcessMailEventsCommand", new { RawMailEventsId = id });

        return req.CreateResponse(HttpStatusCode.OK);
    }
}