using System.Net.Http;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleInjector;

namespace EventRegistrar.Backend.Test.TestInfrastructure
{
    public class IntegrationTestEnvironment
    {
        public IntegrationTestEnvironment()
        {
            var builderKiss4Web = WebHost.CreateDefaultBuilder()
                //.ConfigureAppConfiguration(o => o.AddInMemoryCollection(new[]
                //{
                //    //new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", connectionString),
                //}))
                .ConfigureServices(c =>
                {
                    //c.AddSingleton<IDateTimeProvider>(DateTime);
                })
                .UseStartup<TestStartup>();

            TestServer = new TestServer(builderKiss4Web);
            var container = TestServer.Host.Services.GetService<Container>();
            Scenario = container.GetInstance<TestScenario>();
            Scenario.Create(container).Wait();
        }

        public TestScenario Scenario { get; set; }

        public TestServer TestServer { get; set; }

        public HttpClient GetClient(UserInEventRole role)
        {
            var identifier = role == UserInEventRole.Admin
                ? Scenario.Administrator.IdentityProviderUserIdentifier
                : Scenario.Reader.IdentityProviderUserIdentifier;
            return GetClient(identifier);
        }

        public HttpClient GetClient(string userIdentifier)
        {
            var client = TestServer.CreateClient();
            client.DefaultRequestHeaders.Add(TestGoogleIdentityProvider.TestHeaderUserId, userIdentifier);
            return client;
        }

        public HttpClient GetClient(AuthenticatedUser userToInject)
        {
            var client = TestServer.CreateClient();
            var userSerialized = JsonConvert.SerializeObject(userToInject);
            client.DefaultRequestHeaders.Add(TestGoogleIdentityProvider.TestInjectedUser, userSerialized);
            return client;
        }

        public Container GetServerContainer()
        {
            return TestServer.Host.Services.GetService<Container>();
        }
    }
}