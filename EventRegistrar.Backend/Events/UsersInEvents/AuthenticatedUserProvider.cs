using System;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;
using Microsoft.AspNetCore.Http;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    internal class AuthenticatedUserProvider : IAuthenticatedUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GoogleIdentityProvider _identityProvider;

        public AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor, GoogleIdentityProvider identityProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityProvider = identityProvider;
        }

        public Task<Guid> GetAuthenticatedUserId()
        {
            var idToken = _identityProvider.GetIdentifier(_httpContextAccessor);
            //_httpContextAccessor.HttpContext.Request.Headers.FirstOrDefault()
            return Task.FromResult(new Guid("E24CFA7C-20D7-4AA4-B646-4CB0B1E8D6FC"));
        }
    }
}