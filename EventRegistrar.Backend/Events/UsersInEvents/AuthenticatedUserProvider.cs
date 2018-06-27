using System;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authentication.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    internal class AuthenticatedUserProvider : IAuthenticatedUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityProvider _identityProvider;
        private readonly ILogger _logger;
        private readonly IQueryable<User> _users;

        public AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor,
                                         IIdentityProvider identityProvider,
                                         IQueryable<User> users,
                                         ILogger logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityProvider = identityProvider;
            _users = users;
            _logger = logger;
        }

        public IdentityProvider IdentityProvider => _identityProvider.Provider;

        public string IdentityProviderUserIdentifier => _identityProvider.GetIdentifier(_httpContextAccessor);

        public async Task<Guid?> GetAuthenticatedUserId()
        {
            return new Guid("E24CFA7C-20D7-4AA4-B646-4CB0B1E8D6FC");
            var identifier = _identityProvider.GetIdentifier(_httpContextAccessor);
            var user = await _users.FirstOrDefaultAsync(usr => usr.IdentityProvider == _identityProvider.Provider
                                                            && usr.IdentityProviderUserIdentifier == identifier);
            _logger.LogInformation("Identifier {0}, header count {1}", identifier, _httpContextAccessor?.HttpContext?.Request?.Headers?.Count);
            if (user == null)
            {
                //throw new AuthenticationException($"There is no user {identifier} registered (provider {_identityProvider.Provider})");
                return new Guid();
            }

            return user.Id; //new Guid("E24CFA7C-20D7-4AA4-B646-4CB0B1E8D6FC"));
        }
    }
}