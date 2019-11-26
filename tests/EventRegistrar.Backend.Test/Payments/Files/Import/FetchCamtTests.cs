using System.Threading.Tasks;
using EventRegistrar.Backend.Test.TestInfrastructure;
using Xunit;

namespace EventRegistrar.Backend.Test.Payments.Files.Import
{
    public class FetchCamtTests : IClassFixture<IntegrationTestEnvironment>
    {
        private readonly IntegrationTestEnvironment _integrationTestEnvironment;

        public FetchCamtTests(IntegrationTestEnvironment integrationTestEnvironment)
        {
            _integrationTestEnvironment = integrationTestEnvironment;
        }

        [Fact]
        public async Task ImportCamtFiles()
        {
            var client = _integrationTestEnvironment.GetClient(Events.UsersInEvents.UserInEventRole.Writer);
            var response = await client.PostAsync("api/events/tev/fetchBankStatementFiles", null);
            response.EnsureSuccessStatusCode();
        }
    }
}