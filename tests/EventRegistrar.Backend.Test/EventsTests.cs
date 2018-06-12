using System.Threading.Tasks;
using EventRegistrator.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class EventsTests : IClassFixture<WebApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _factory;

        public EventsTests(WebApplicationFactory<TestStartup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetEventsOfUser()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("me/events");
            response.EnsureSuccessStatusCode();
        }
    }
}