using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Test.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class AccessToEventTests : IClassFixture<IntegrationTestEnvironment>
    {
        private readonly IntegrationTestEnvironment _testEnvironment;

        public AccessToEventTests(IntegrationTestEnvironment testEnvironment)
        {
            _testEnvironment = testEnvironment;
        }

        [Fact]
        public async Task RequestAccessToEvent()
        {
            const string requestText = "Please add me!";
            var eventToRequest = _testEnvironment.Scenario.OtherCurrentEvent;
            var client = _testEnvironment.GetClient(UserInEventRole.Reader);

            // Act
            var response = await client.PostAsJsonAsync($"api/events/{eventToRequest.Acronym}/requestAccess", requestText);

            // Assert
            response.EnsureSuccessStatusCode();
            var container = _testEnvironment.GetServerContainer();
            using (new EnsureExecutionScope(container))
            {
                var requests = container.GetInstance<IQueryable<AccessToEventRequest>>();
                var readerUser = _testEnvironment.Scenario.Reader;
                var request = await requests.FirstOrDefaultAsync(req => req.EventId == eventToRequest.Id
                                                                     && req.Identifier == readerUser.IdentityProviderUserIdentifier
                                                                     && req.IdentityProvider == readerUser.IdentityProvider);
                request.ShouldNotBeNull();
                request.Email.ShouldBe(readerUser.Email);
                request.FirstName.ShouldBe(readerUser.FirstName);
                request.LastName.ShouldBe(readerUser.LastName);
                request.RequestText.ShouldBe(requestText);
            }
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