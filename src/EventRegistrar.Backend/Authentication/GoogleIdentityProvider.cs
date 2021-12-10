using System.IdentityModel.Tokens.Jwt;

using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Authentication;

public class GoogleIdentityProvider : IIdentityProvider
{
    public const string HeaderKeyIdToken = "X-MS-TOKEN-GOOGLE-ID-TOKEN";

    public IdentityProvider Provider => IdentityProvider.Google;

    public (IdentityProvider? Provider, string? Identifier) GetIdentifier(IHttpContextAccessor contextAccessor)
    {
        var idTokenString = contextAccessor.HttpContext?.Request.Headers[HeaderKeyIdToken].FirstOrDefault();
        if (idTokenString != null)
        {
            var token = new JwtSecurityToken(idTokenString);
            return (IdentityProvider.Google, token.Subject);
        }

        return (null, null);
    }

    public AuthenticatedUser GetUser(IHttpContextAccessor contextAccessor)
    {
        var headers = contextAccessor.HttpContext?.Request.Headers;
        var idTokenString = headers?[HeaderKeyIdToken].FirstOrDefault();
        if (idTokenString != null)
        {
            var token = new JwtSecurityToken(idTokenString);

            var firstName = token.GetClaim("given_name");
            var lastName = token.GetClaim("family_name");
            var email = token.GetClaim("email");
            return new AuthenticatedUser(Provider, token.Subject, firstName, lastName, email);
        }

        return AuthenticatedUser.None;
    }
}