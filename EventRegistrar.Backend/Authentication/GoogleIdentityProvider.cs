using System.IdentityModel.Tokens.Jwt;
using EventRegistrar.Backend.Events.UsersInEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Authentication
{
    public class GoogleIdentityProvider : IIdentityProvider
    {
        public const string HeaderKeyIdToken = "X-MS-TOKEN-GOOGLE-ID-TOKEN";
        private readonly ILogger _logger;

        public GoogleIdentityProvider(ILogger logger)
        {
            _logger = logger;
        }

        public IdentityProvider Provider => IdentityProvider.Google;

        public string GetIdentifier(IHttpContextAccessor contextAccessor)
        {
            var idTokenString = (string)contextAccessor.HttpContext?.Request?.Headers?[HeaderKeyIdToken];
            if (idTokenString != null)
            {
                var token = new JwtSecurityToken(idTokenString);
                //_logger.Log(LogLevel.Information, "token {0}, subject {1}, issuer {2}", idTokenString, token.Subject, token.Issuer);
                return token.Subject;
            }

            return null;
        }

        public AuthenticatedUser GetUser(IHttpContextAccessor contextAccessor)
        {
            var headers = contextAccessor.HttpContext?.Request?.Headers;
            var idTokenString = (string)headers?[HeaderKeyIdToken];
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
}