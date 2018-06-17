using EventRegistrar.Backend.Authentication;
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
    }
}