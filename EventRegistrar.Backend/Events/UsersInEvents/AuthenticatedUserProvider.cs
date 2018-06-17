using System;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    internal class AuthenticatedUserProvider : IAuthenticatedUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GoogleIdentityProvider _identityProvider;
        private readonly ILogger _logger;

        public AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor,
                                         GoogleIdentityProvider identityProvider,
                                         ILogger logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityProvider = identityProvider;
            _logger = logger;
        }

        public Task<Guid> GetAuthenticatedUserId()
        {
            var idToken = _identityProvider.GetIdentifier(_httpContextAccessor);
            _logger.Log(LogLevel.Information, "token {0}", idToken);
            //_httpContextAccessor.HttpContext.Request.Headers.FirstOrDefault()
            return Task.FromResult(new Guid("E24CFA7C-20D7-4AA4-B646-4CB0B1E8D6FC"));
        }
    }
}