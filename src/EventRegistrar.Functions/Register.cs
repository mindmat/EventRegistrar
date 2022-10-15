using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventRegistrar.Functions;

public static class Register
{
    [Function(nameof(Register))]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventAcronym}/registrationforms/{formId}/registrations/{registrationId}")]
        HttpRequestData req,
        string eventAcronym,
        string formId,
        string registrationId)
    {
        var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                               .Build();

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var connectionString = config.GetConnectionString("DefaultConnection");
        var rawRegistrationId = Guid.NewGuid();
        await using (var connection = new SqlConnection(connectionString))
        {
            const string insertQuery = @"INSERT INTO dbo.RawRegistrations(Id, EventAcronym, ReceivedMessage, FormExternalIdentifier, RegistrationExternalIdentifier, Created) "
                                     + @"VALUES (@Id, @EventAcronym, @ReceivedMessage, @FormExternalIdentifier, @RegistrationExternalIdentifier, @Created)";
            var parameters = new
                             {
                                 Id = rawRegistrationId,
                                 EventAcronym = eventAcronym,
                                 ReceivedMessage = requestBody,
                                 FormExternalIdentifier = formId,
                                 RegistrationExternalIdentifier = registrationId,
                                 Created = DateTimeOffset.Now
                             };

            await connection.ExecuteAsync(insertQuery, parameters);
        }

        await CommandQueue.SendCommand(new { RawRegistrationId = rawRegistrationId });

        return req.CreateResponse(HttpStatusCode.OK);
    }
}