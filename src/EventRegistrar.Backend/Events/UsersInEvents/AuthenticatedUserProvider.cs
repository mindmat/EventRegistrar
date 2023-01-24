using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authentication.Users;

namespace EventRegistrar.Backend.Events.UsersInEvents;

internal class AuthenticatedUserProvider : IAuthenticatedUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IIdentityProvider _identityProvider;
    private readonly IQueryable<User> _users;

    public AuthenticatedUserProvider(IHttpContextAccessor httpContextAccessor,
                                     IIdentityProvider identityProvider,
                                     IQueryable<User> users)
    {
        _httpContextAccessor = httpContextAccessor;
        _identityProvider = identityProvider;
        _users = users;
    }

    public AuthenticatedUser GetAuthenticatedUser()
    {
        return _identityProvider.GetUser(_httpContextAccessor);
    }

    public async Task<Guid?> GetAuthenticatedUserId()
    {
        var identifier = _identityProvider.GetIdentifier(_httpContextAccessor);
        if (identifier != null)
        {
            return await _users.Where(usr => usr.IdentityProvider == identifier.Value.Provider
                                          && usr.IdentityProviderUserIdentifier == identifier.Value.Identifier)
                               .Select(usr => (Guid?)usr.Id)
                               .FirstOrDefaultAsync();
        }

        return null;
    }
}