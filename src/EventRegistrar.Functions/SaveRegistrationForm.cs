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

public static class SaveRegistrationForm
{
    [Function(nameof(SaveRegistrationForm))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventAcronym}/registrationforms/{formId}")] HttpRequestData req,
                                                   string eventAcronym,
                                                   string formId)
    {
        var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                               .Build();

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var connectionString = config.GetConnectionString("DefaultConnection");
        var rawRegistrationFormId = Guid.NewGuid();
        await using (var connection = new SqlConnection(connectionString))
        {
            const string insertQuery = "INSERT INTO dbo.RawRegistrationForms(Id, EventAcronym, ReceivedMessage, FormExternalIdentifier, Created) "
                                     + "VALUES (@Id, @EventAcronym, @ReceivedMessage, @FormExternalIdentifier, @Created)";
            var parameters = new
                             {
                                 Id = rawRegistrationFormId,
                                 EventAcronym = eventAcronym,
                                 ReceivedMessage = requestBody,
                                 FormExternalIdentifier = formId,
                                 Created = DateTimeOffset.Now
                             };

            await connection.ExecuteAsync(insertQuery, parameters);
        }

        await CommandQueue.SendCommand("EventRegistrar.Backend.RegistrationForms.GoogleForms.ProcessRawRegistrationFormReceivedCommand", new { RawRegistrationFormId = rawRegistrationFormId });

        return req.CreateResponse(HttpStatusCode.OK);
    }
}