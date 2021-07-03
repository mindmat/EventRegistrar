using System;
using System.IO;
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
    public static class Register
    {
        [FunctionName("Register")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventAcronym}/registrationforms/{formId}/registrations/{registrationId}")]
                                                     HttpRequest req,
                                                     ILogger log,
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
                const string insertQuery = @"INSERT INTO dbo.RawRegistrations(Id, EventAcronym, ReceivedMessage, FormExternalIdentifier, RegistrationExternalIdentifier, Created) " +
                                           @"VALUES (@Id, @EventAcronym, @ReceivedMessage, @FormExternalIdentifier, @RegistrationExternalIdentifier, @Created)";
                var parameters = new
                {
                    Id = rawRegistrationId,
                    EventAcronym = eventAcronym,
                    ReceivedMessage = requestBody,
                    FormExternalIdentifier = formId,
                    RegistrationExternalIdentifier = registrationId,
                    Created = DateTime.UtcNow
                };

                await connection.ExecuteAsync(insertQuery, parameters);
            }

            await ServiceBusClient.SendCommand(new { RawRegistrationId = rawRegistrationId }, "processrawregistration");

            return new OkResult();
        }
    }
}