using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
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
            var client = _testEnvironment.GetClient(UserInEventRole.Reader);

            var response = await client.GetAsync("api/me/events");
            response.EnsureSuccessStatusCode();
            var events = (await response.Content.ReadAsAsync<IEnumerable<UserInEventDisplayItem>>()).ToList();

            events.ShouldNotBeNull();
            events.Count.ShouldBe(2);
            events.ShouldContain(evt => evt.EventAcronym == _testEnvironment.Scenario.TestEvent.Acronym && evt.Role == UserInEventRole.Writer);
            events.ShouldContain(evt => evt.EventAcronym == _testEnvironment.Scenario.OtherOwnEvent.Acronym && evt.Role == UserInEventRole.Reader);
        }

        [Theory]
        [InlineData("other", false, false, new[] { "cev" })]
        [InlineData("other", false, true, new[] { "cev", "ooe" })]
        [InlineData("future", true, false, new[] { "fev" })]
        public async Task SearchEvents(string searchString, bool includeRequestedEvents, bool includeAuthorizedEvents, string[] expectedSearchResultAcronyms)
        {
            var client = _testEnvironment.GetClient(UserInEventRole.Reader);

            var response = await client.GetAsync($"api/events?searchString={searchString}&includeRequestedEvents={includeRequestedEvents}&includeAuthorizedEvents={includeAuthorizedEvents}");
            response.EnsureSuccessStatusCode();
            var events = (await response.Content.ReadAsAsync<IEnumerable<EventSearchResult>>()).ToList();

            events.ShouldNotBeNull();
            events.Count.ShouldBe(expectedSearchResultAcronyms.Length);
            foreach (var acronym in expectedSearchResultAcronyms)
            {
                events.ShouldContain(evt => evt.Acronym == acronym);
            }
        }
    }
}