using System.IdentityModel.Tokens.Jwt;
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

                _logger.Log(LogLevel.Information, "token {0}, subject {1}", idTokenString, token.Subject);
                return token.Subject;
            }

            return null;
        }
    }
}