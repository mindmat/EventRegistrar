using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using SimpleInjector;

using System;

namespace EventRegistrar.Backend.Test.TestInfrastructure
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override DbContextOptions<EventRegistratorDbContext> GetDbOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
            optionsBuilder.UseInMemoryDatabase($"InMemoryDb{Guid.NewGuid()}");
            return optionsBuilder.Options;
        }

        protected override void SetIdentityProvider(Container container)
        {
            container.Register<IIdentityProvider, TestGoogleIdentityProvider>();
            container.RegisterSingleton<TestScenario>();
        }
    }
}