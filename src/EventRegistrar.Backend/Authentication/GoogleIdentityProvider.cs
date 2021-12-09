using System.IdentityModel.Tokens.Jwt;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Authentication;

public class GoogleIdentityProvider : IIdentityProvider
{
    public const string HeaderKeyIdToken = "X-MS-TOKEN-GOOGLE-ID-TOKEN";
    private readonly ILogger _logger;

    public GoogleIdentityProvider(ILogger logger)
    {
        _logger = logger;
    }

    public IdentityProvider Provider => IdentityProvider.Google;

    public string? GetIdentifier(IHttpContextAccessor contextAccessor)
    {
        var idTokenString = contextAccessor.HttpContext?.Request?.Headers?[HeaderKeyIdToken].FirstOrDefault();
        if (idTokenString != null)
        {
            var token = new JwtSecurityToken(idTokenString);
            _logger.LogInformation("token {0}, subject {1}, issuer {2}", idTokenString, token.Subject, token.Issuer);
            return token.Subject;
        }

        _logger.LogInformation("no token found in request. headers present: {0}",
            contextAccessor.HttpContext?.Request?.Headers?.Select(hdr => $"{hdr.Key}: {hdr.Value}")?.StringJoin());
        return null;
    }

    public AuthenticatedUser GetUser(IHttpContextAccessor contextAccessor)
    {
        var headers = contextAccessor.HttpContext?.Request?.Headers;
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