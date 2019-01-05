using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EventRegistrar.Backend.Test.TestInfrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder() => WebHost.CreateDefaultBuilder()
            .UseApplicationInsights()
            .UseStartup<TestStartup>();
    }
}