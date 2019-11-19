using System;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;
using EventRegistrar.Backend.Test.Infrastructure;
using EventRegistrar.Backend.Test.TestInfrastructure;
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
            var eventToRequest = _testEnvironment.Scenario.PastEvent;
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
                // request.RequestText.ShouldBe(requestText);
            }
        }

        [Theory]
        [InlineData("cev", RequestResponse.Granted, UserInEventRole.Reader, "Welcome!", TestScenario.IdentityProviderUserIdentifierReader, null)]
        [InlineData("fev", RequestResponse.Denied, UserInEventRole.Reader, "Nope", TestScenario.IdentityProviderUserIdentifierReader, null)]
        [InlineData("fev", RequestResponse.Denied, UserInEventRole.Writer, "Nope", null, "new.user@gmail.com")]
        [InlineData("cev", RequestResponse.Granted, UserInEventRole.Writer, "OK", null, "new.user2@gmail.com")]
        public async Task RespondToAccessToEventRequest(string eventAcronym,
                                                        RequestResponse response,
                                                        UserInEventRole role,
                                                        string responseText,
                                                        string userIdentifier,
                                                        string newUserIdentifier)
        {
            // Arrange
            var unknownUser = new AuthenticatedUser(IdentityProvider.Google,
                                                    newUserIdentifier,
                                                    "New",
                                                    "User",
                                                    newUserIdentifier);
            var requestorClient = userIdentifier != null
                ? _testEnvironment.GetClient(userIdentifier)
                : _testEnvironment.GetClient(unknownUser);

            var requestHttpResponse = await requestorClient.PostAsJsonAsync($"api/events/{eventAcronym}/requestAccess", string.Empty);
            requestHttpResponse.EnsureSuccessStatusCode();
            var accessRequestId = await requestHttpResponse.Content.ReadAsAsync<Guid>();
            var responderClient = _testEnvironment.GetClient(UserInEventRole.Admin);

            // Act
            var responseDto = new RequestResponseDto
            {
                Response = response,
                Role = role,
                Text = responseText
            };
            var httpResponse = await responderClient.PostAsJsonAsync($"api/accessrequest/{accessRequestId}/respond", responseDto);

            // Assert
            httpResponse.EnsureSuccessStatusCode();
            var container = _testEnvironment.GetServerContainer();
            using (new EnsureExecutionScope(container))
            {
                var dbRequest = await container.GetInstance<IQueryable<AccessToEventRequest>>().FirstOrDefaultAsync(req => req.Id == accessRequestId);
                dbRequest.ShouldNotBeNull();
                dbRequest.Response.ShouldBe(response);
                dbRequest.ResponseText.ShouldBe(responseText);

                var user = dbRequest.UserId_Requestor.HasValue
                    ? await container.GetInstance<IQueryable<User>>()
                                     .FirstOrDefaultAsync(usr => usr.Id == dbRequest.UserId_Requestor)
                    : await container.GetInstance<IQueryable<User>>()
                                     .FirstOrDefaultAsync(usr => usr.IdentityProvider == dbRequest.IdentityProvider
                                                              && usr.IdentityProviderUserIdentifier == dbRequest.Identifier);
                var userIdRequestor = dbRequest.UserId_Requestor ?? user?.Id;
                UserInEvent userAccess = null;
                if (userIdRequestor.HasValue)
                {
                    userAccess = await container.GetInstance<IQueryable<UserInEvent>>().FirstOrDefaultAsync(uie => uie.UserId == user.Id
                                                                                                                && uie.Event.Acronym == eventAcronym);
                }
                if (response == RequestResponse.Granted)
                {
                    userAccess.ShouldNotBeNull();
                    userAccess.Role.ShouldBe(role);

                    user.ShouldNotBeNull();
                    user.FirstName.ShouldBe(dbRequest.FirstName);
                    user.LastName.ShouldBe(dbRequest.LastName);
                    user.Email.ShouldBe(dbRequest.Email);
                }
                else
                {
                    userAccess.ShouldBeNull();
                    if (userIdentifier == null)
                    {
                        user.ShouldBeNull();
                    }
                }
            }
        }
    }
}