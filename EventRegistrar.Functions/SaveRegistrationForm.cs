using System;
using System.IO;
using System.Threading.Tasks;

using Dapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventRegistrar.Functions
{
    public static class SaveRegistrationForm
    {
        [FunctionName("SaveRegistrationForm")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventAcronym}/registrationforms/{formId}")]
                                                     HttpRequest req,
                                                     string eventAcronym,
                                                     string formId,
                                                     TraceWriter log)
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                                   .Build();

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var connectionString = config.GetConnectionString("DefaultConnection");
            await using (var connection = new SqlConnection(connectionString))
            {
                const string insertQuery = @"INSERT INTO dbo.RawRegistrationForms(Id, EventAcronym, ReceivedMessage, FormExternalIdentifier, Created) " +
                                           @"VALUES (@Id, @EventAcronym, @ReceivedMessage, @FormExternalIdentifier, @Created)";
                var parameters = new
                {
                    Id = Guid.NewGuid(),
                    EventAcronym = eventAcronym,
                    ReceivedMessage = requestBody,
                    FormExternalIdentifier = formId,
                    Created = DateTime.UtcNow
                };

                await connection.ExecuteAsync(insertQuery, parameters);
            }

            return new OkResult();
        }
    }
}