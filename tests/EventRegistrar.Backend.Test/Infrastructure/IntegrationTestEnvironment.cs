using System.Net.Http;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace EventRegistrar.Backend.Test.Infrastructure
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
            Scenario = new TestScenario();
            var container = TestServer.Host.Services.GetService<Container>();
            Scenario.Create(container).Wait();
        }

        public TestScenario Scenario { get; set; }

        public TestServer TestServer { get; set; }

        public HttpClient GetClient(UserInEventRole role)
        {
            var client = TestServer.CreateClient();
            var identifier = role == UserInEventRole.Admin
                ? Scenario.Administrator.IdentityProviderUserIdentifier
                : Scenario.Reader.IdentityProviderUserIdentifier;
            client.DefaultRequestHeaders.Add(TestGoogleIdentityProvider.TestHeaderUserId, identifier);
            return client;
        }
    }
}