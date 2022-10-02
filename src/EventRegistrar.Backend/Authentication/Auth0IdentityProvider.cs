using EventRegistrar.Backend.Events.UsersInEvents;

using System.Security.Claims;

namespace EventRegistrar.Backend.Authentication;

public class Auth0IdentityProvider : IIdentityProvider
{
    private readonly IDictionary<string, IdentityProvider> _providers = new Dictionary<string, IdentityProvider>
                                                                        {
                                                                            { "google-oauth2", IdentityProvider.Google }
                                                                        };

    public (IdentityProvider Provider, string Identifier)? GetIdentifier(IHttpContextAccessor contextAccessor)
    {
        if (contextAccessor.HttpContext?.User.Identity is ClaimsIdentity { IsAuthenticated: true } claimsIdentity)
        {
            var idString = claimsIdentity.Claims.FirstOrDefault(clm => clm.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                                         ?.Value;
            var parts = idString?.Split('|');
            if (parts?.Length == 2)
            {
                return (_providers[parts[0]], parts[1]);
            }
        }

        return null;
    }

    public AuthenticatedUser GetUser(IHttpContextAccessor contextAccessor)
    {
        var extract = GetIdentifier(contextAccessor);
        return extract == null
                   ? AuthenticatedUser.None
                   : new AuthenticatedUser(extract.Value.Provider, extract.Value.Identifier, string.Empty, string.Empty, string.Empty);
    }
}