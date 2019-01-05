using System.Threading.Tasks;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Test.TestInfrastructure;
using Xunit;

namespace EventRegistrar.Backend.Test.Mailing.Import
{
    public class ImportMailsTests : IClassFixture<IntegrationTestEnvironment>
    {
        private readonly IntegrationTestEnvironment _integrationTestEnvironment;

        public ImportMailsTests(IntegrationTestEnvironment integrationTestEnvironment)
        {
            _integrationTestEnvironment = integrationTestEnvironment;
        }

        [Fact]
        public async Task ImportMails()
        {
            var client = _integrationTestEnvironment.GetClient(UserInEventRole.Reader);

            var response = await client.PostAsync("api/events/tev/mails/import", null);
            response.EnsureSuccessStatusCode();
        }
    }
}