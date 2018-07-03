using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                var issuer = token.Issuer;
                _logger.Log(LogLevel.Information, "token {0}, subject {1}, issuer {2}", idTokenString, token.Subject, issuer);
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

                var firstName = (string)headers?[ClaimTypes.GivenName];
                var lastName = (string)headers?[ClaimTypes.Name];
                var email = (string)headers?[ClaimTypes.Email];
                return new AuthenticatedUser(Provider, token.Subject, firstName, lastName, email);
            }

            return null;
        }
    }
}