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

        [Fact]
        public async Task RespondToAccessToEventRequest()
        {
            const string requestText = "Response";
            var client = _testEnvironment.GetClient(UserInEventRole.Reader);
            var reader = _testEnvironment.Scenario.Reader;
            var request = _testEnvironment.Scenario.AccessRequest;

            // Act
            var responseDto = new RequestResponseDto
            {
                Response = RequestResponse.Granted,
                Role = UserInEventRole.Reader,
                Text = requestText
            };
            var response = await client.PostAsJsonAsync($"api/accessrequest/{request.Id}/respond", responseDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var container = _testEnvironment.GetServerContainer();
            using (new EnsureExecutionScope(container))
            {
                var dbRequest = await container.GetInstance<IQueryable<AccessToEventRequest>>().FirstOrDefaultAsync(req => req.Id == request.Id);
                dbRequest.ShouldNotBeNull();
                dbRequest.Response.ShouldBe(responseDto.Response);
                dbRequest.ResponseText.ShouldBe(responseDto.Text);

                var userAccess = await container.GetInstance<IQueryable<UserInEvent>>().FirstOrDefaultAsync(uie => uie.UserId == reader.Id
                                                                                                                && uie.EventId == request.EventId);
                userAccess.ShouldNotBeNull();
                userAccess.Role.ShouldBe(responseDto.Role);
            }
        }
    }
}