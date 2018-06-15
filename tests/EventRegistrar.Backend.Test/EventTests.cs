using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Test.Infrastructure;
using EventRegistrator.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleInjector;
using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class EventTests : IClassFixture<WebApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _factory;

        public EventTests(WebApplicationFactory<TestStartup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetEventsOfUser()
        {
            var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Add(GoogleIdentityProvider.HeaderKeyIdToken, "abc");
            var container = _factory.Server.Host.Services.GetService<Container>();
            var scenario = new TestScenario();
            scenario.Create(container);

            var response = await client.GetAsync("api/me/events");
            response.EnsureSuccessStatusCode();
            var events = (await response.Content.ReadAsAsync<IEnumerable<UserInEventDisplayItem>>()).ToList();

            events.ShouldNotBeNull();
            events.Count.ShouldBe(2);
            events.ShouldContain(evt => evt.EventAcronym == scenario.TestEvent.Acronym && evt.Role == UserInEventRole.User);
            events.ShouldContain(evt => evt.EventAcronym == scenario.OtherOwnEvent.Acronym && evt.Role == UserInEventRole.User);
        }
    }
}