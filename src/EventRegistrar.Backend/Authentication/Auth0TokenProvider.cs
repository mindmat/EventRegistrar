using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Authentication;

public class Auth0TokenProvider(SingletonConfigurationFeature<Auth0Configuration> configuration,
                                SecretReader secretReader)
{
    private string? _token;
    private DateTimeOffset? _expires;

    public async Task<string?> GetToken()
    {
        if (_token != null && _expires != null && (_expires.Value - DateTimeOffset.Now).TotalSeconds > 60)
        {
            return _token;
        }

        var config = configuration.Configuration;
        var clientId = await secretReader.GetSecret(config.ClientIdKey);
        var clientSecret = await secretReader.GetSecret(config.ClientSecretKey);
        if (clientId == null || clientSecret == null)
        {
            throw new Exception("Could not retrieve client id or client secret");
        }

        // Get token: https://auth0.com/docs/secure/tokens/access-tokens/get-management-api-access-tokens-for-production
        var data = new Dictionary<string, string>
                   {
                       { "grant_type", "client_credentials" },
                       { "client_id", clientId },
                       { "client_secret", clientSecret },
                       { "audience", config.Audience }
                   };
        var requestToken = new HttpRequestMessage(HttpMethod.Post, config.TokenUrl)
                           {
                               Content = new FormUrlEncodedContent(data)
                           };
        using var client = new HttpClient();
        var responseToken = await client.SendAsync(requestToken);
        if (responseToken.IsSuccessStatusCode)
        {
            var contentToken = await responseToken.Content.ReadFromJsonAsync<Auth0Token>();
            _token = contentToken?.access_token;
            _expires = contentToken == null
                           ? null
                           : DateTimeOffset.Now.AddSeconds(contentToken.expires_in);
        }

        return null;
    }

    private class Auth0Token
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
    };
}