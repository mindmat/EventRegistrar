using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Test.Infrastructure;
using Shouldly;
using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class EventTests : IClassFixture<IntegrationTestEnvironment>
    {
        private readonly IntegrationTestEnvironment _testEnvironment;

        public EventTests(IntegrationTestEnvironment testEnvironment)
        {
            _testEnvironment = testEnvironment;
        }

        [Fact]
        public async Task GetEventsOfUser()
        {
            var client = _testEnvironment.GetClient();

            var response = await client.GetAsync("api/me/events");
            response.EnsureSuccessStatusCode();
            var events = (await response.Content.ReadAsAsync<IEnumerable<UserInEventDisplayItem>>()).ToList();

            events.ShouldNotBeNull();
            events.Count.ShouldBe(2);
            events.ShouldContain(evt => evt.EventAcronym == _testEnvironment.Scenario.TestEvent.Acronym && evt.Role == UserInEventRole.Writer);
            events.ShouldContain(evt => evt.EventAcronym == _testEnvironment.Scenario.OtherOwnEvent.Acronym && evt.Role == UserInEventRole.Reader);
        }
    }
}