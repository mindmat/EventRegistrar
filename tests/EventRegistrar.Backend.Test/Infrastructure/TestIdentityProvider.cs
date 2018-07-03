using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Events.UsersInEvents;
using Microsoft.AspNetCore.Http;

namespace EventRegistrar.Backend.Test.Infrastructure
{
    public class TestGoogleIdentityProvider : IIdentityProvider
    {
        public const string TestHeaderUserId = "TestHeaderUserId";
        public IdentityProvider Provider => IdentityProvider.Google;

        public string GetIdentifier(IHttpContextAccessor contextAccessor)
        {
            return contextAccessor.HttpContext?.Request?.Headers?[TestHeaderUserId];
        }

        public AuthenticatedUser GetUser(IHttpContextAccessor httpContextAccessor)
        {
            var identifier = GetIdentifier(httpContextAccessor);
            return new AuthenticatedUser(Provider, identifier, null, null, null);
        }
    }
}