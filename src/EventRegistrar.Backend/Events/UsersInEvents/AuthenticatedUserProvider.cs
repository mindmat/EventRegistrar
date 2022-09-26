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
        var (provider, identifier) = _identityProvider.GetIdentifier(_httpContextAccessor);
        if (provider != null && identifier != null)
        {
            var user = await _users.FirstOrDefaultAsync(usr => usr.IdentityProvider == provider
                                                            && usr.IdentityProviderUserIdentifier == identifier);
            if (user == null)
            {
                //return new Guid("73B167CE-61CC-46AC-BC7D-F72A1EA5D7C9");
                return null;
            }

            return user.Id;
        }

        return null;
    }
}