using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Functions
{
    public static class GetRegistrationExternalIdentifiersOfForm
    {
        [FunctionName("GetRegistrationExternalIdentifiersOfForm")]
        public static async Task<IEnumerable<string>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrationforms/{formId}/RegistrationExternalIdentifiers")]
            HttpRequest req,
            string formId,
            ILogger log)
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                                   .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsync<string>("SELECT RegistrationExternalIdentifier " +
                                                           "FROM dbo.RawRegistrations " +
                                                           "WHERE FormExternalIdentifier = @formId", new { formId });
            }
        }
    }
}