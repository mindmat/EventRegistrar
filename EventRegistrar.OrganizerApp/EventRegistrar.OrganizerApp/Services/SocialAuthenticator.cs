using System;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace EventRegistrar.OrganizerApp.Services
{
    public class SocialAuthenticator
    {
        public async Task Authenticate()
        {
            var authResult = await WebAuthenticator.AuthenticateAsync(
                new Uri("https://eventregistrar.azurewebsites.net/.auth/login/google/callback"),
                new Uri("eventregistrar://"));

            var accessToken = authResult?.AccessToken;
        }
    }
}