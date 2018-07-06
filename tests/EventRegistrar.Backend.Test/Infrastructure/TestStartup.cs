using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;
using EventRegistrator.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SimpleInjector;

namespace EventRegistrar.Backend.Test.Infrastructure
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override DbContextOptions<EventRegistratorDbContext> GetDbOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
            optionsBuilder.UseInMemoryDatabase("InMemoryDb");
            return optionsBuilder.Options;
        }

        protected override void SetIdentityProvider(Container container)
        {
            container.Register<IIdentityProvider, TestGoogleIdentityProvider>();
            container.RegisterSingleton<TestScenario>();
        }
    }
}