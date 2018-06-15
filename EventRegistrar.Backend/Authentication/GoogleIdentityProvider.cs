using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace EventRegistrar.Backend.Authentication
{
    public class GoogleIdentityProvider : IIdentityProvider
    {
        public const string HeaderKeyIdToken = "X-MS-TOKEN-GOOGLE-ID-TOKEN";
        public IdentityProvider Provider => IdentityProvider.Google;

        public string GetIdentifier(IHttpContextAccessor contextAccessor)
        {
            var idToken = contextAccessor.HttpContext?.Request?.Headers?[HeaderKeyIdToken];
            Trace.WriteLine(idToken);
            return idToken;
        }
    }
}