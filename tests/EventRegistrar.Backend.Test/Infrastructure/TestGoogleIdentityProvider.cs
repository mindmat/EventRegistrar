using System;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;
using Microsoft.AspNetCore.Http;

namespace EventRegistrar.Backend.Test.Infrastructure
{
    public class TestGoogleIdentityProvider : IIdentityProvider
    {
        private readonly TestScenario _testScenario;

        public TestGoogleIdentityProvider(TestScenario testScenario)
        {
            _testScenario = testScenario;
        }

        public const string TestHeaderUserId = "TestHeaderUserId";
        public IdentityProvider Provider => IdentityProvider.Google;

        public string GetIdentifier(IHttpContextAccessor contextAccessor)
        {
            return contextAccessor.HttpContext?.Request?.Headers?[TestHeaderUserId];
        }

        public AuthenticatedUser GetUser(IHttpContextAccessor httpContextAccessor)
        {
            var identifier = GetIdentifier(httpContextAccessor);
            if (identifier == _testScenario.Administrator.IdentityProviderUserIdentifier)
            {
                return new AuthenticatedUser(Provider,
                                             _testScenario.Administrator.IdentityProviderUserIdentifier,
                                             _testScenario.Administrator.FirstName,
                                             _testScenario.Administrator.LastName,
                                             _testScenario.Administrator.Email);
            }

            if (identifier == _testScenario.Reader.IdentityProviderUserIdentifier)
            {
                return new AuthenticatedUser(Provider,
                                             _testScenario.Reader.IdentityProviderUserIdentifier,
                                             _testScenario.Reader.FirstName,
                                             _testScenario.Reader.LastName,
                                             _testScenario.Reader.Email);
            }
            return AuthenticatedUser.None;
        }
    }
}