using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;
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

        [Theory]
        [InlineData(TestScenario.IdentityProviderUserIdentifierReader, new[] { "tev", "ooe" }, new[] { UserInEventRole.Writer, UserInEventRole.Reader })]
        public async Task GetEventsOfUser(string userIdentifier, string[] expectedEventAcronyms, UserInEventRole[] expectedRoles)
        {
            var client = _testEnvironment.GetClient(userIdentifier);

            var response = await client.GetAsync("api/me/events");
            response.EnsureSuccessStatusCode();
            var events = (await response.Content.ReadAsAsync<IEnumerable<UserInEventDisplayItem>>()).ToList();

            events.ShouldNotBeNull();
            events.Count.ShouldBe(expectedEventAcronyms.Length);
            for (var i = 0; i < expectedEventAcronyms.Length; i++)
            {
                events.ShouldContain(evt => evt.EventAcronym == expectedEventAcronyms[i]
                                         && evt.Role == expectedRoles[i]);
            }
        }

        [Theory]
        [InlineData("other", false, false, new[] { "cev" })]
        [InlineData("other", false, true, new[] { "cev", "ooe" })]
        [InlineData("future", false, false, new string[] { })]
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