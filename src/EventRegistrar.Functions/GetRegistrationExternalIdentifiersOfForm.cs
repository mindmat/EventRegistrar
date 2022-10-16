using System.Net;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventRegistrar.Functions;

public static class GetRegistrationExternalIdentifiersOfForm
{
    [Function(nameof(GetRegistrationExternalIdentifiersOfForm))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrationforms/{formId}/RegistrationExternalIdentifiers")] HttpRequestData req,
                                                   string formId)
    {
        var config = new ConfigurationBuilder().AddEnvironmentVariables()
                                               .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);
        var ids = await connection.QueryAsync<string>("SELECT RegistrationExternalIdentifier FROM dbo.RawRegistrations WHERE FormExternalIdentifier = @formId", new { formId });
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ids);
        return response;
    }
}