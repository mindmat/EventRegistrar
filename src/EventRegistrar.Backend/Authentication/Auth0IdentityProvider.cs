using System.Net.Http.Headers;
using System.Security.Claims;

using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Authentication;

public class Auth0IdentityProvider : IIdentityProvider
{
    private readonly Auth0TokenProvider _tokenProvider;

    public Auth0IdentityProvider(Auth0TokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public (IdentityProvider Provider, string Identifier)? GetIdentifier(IHttpContextAccessor contextAccessor)
    {
        if (contextAccessor.HttpContext?.User.Identity is ClaimsIdentity { IsAuthenticated: true } claimsIdentity)
        {
            var identifier = claimsIdentity.Claims.FirstOrDefault(clm => clm.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                                           ?.Value;
            if (identifier != null)
            {
                return (IdentityProvider.Auth0, identifier);
            }
        }

        return null;
    }

    public AuthenticatedUser GetUser(IHttpContextAccessor contextAccessor)
    {
        var extract = GetIdentifier(contextAccessor);
        return extract == null
                   ? AuthenticatedUser.None
                   : new AuthenticatedUser(extract.Value.Provider, extract.Value.Identifier);
    }

    public async Task<ExternalUserDetails?> GetUserDetails(string identifier)
    {
        var token = await _tokenProvider.GetToken();
        if (token != null)
        {
            // Get user details: https://auth0.com/docs/manage-users/user-search/retrieve-users-with-get-users-by-id-endpoint
            var client = new HttpClient();
            var requestUser = new HttpRequestMessage(HttpMethod.Get, $"https://eventregistrar.eu.auth0.com/api/v2/users/{identifier}")
                              {
                                  Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                              };
            var responseUser = await client.SendAsync(requestUser);
            var contentUser = await responseUser.Content.ReadFromJsonAsync<Auth0User>();

            return new ExternalUserDetails
                   {
                       IdentityProvider = IdentityProvider.Auth0,
                       ExternalIdentifier = identifier,
                       FirstName = contentUser?.given_name,
                       LastName = contentUser?.family_name,
                       Email = contentUser?.email,
                       AvatarUrl = contentUser?.picture
                   };
        }

        return null;
    }


    private class Auth0User
    {
        public string email { get; set; }
        public string username { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string picture { get; set; }
    };
}