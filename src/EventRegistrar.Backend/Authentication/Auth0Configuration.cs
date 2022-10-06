using EventRegistrar.Backend.Infrastructure.Configuration;

using Microsoft.IdentityModel.Tokens;

namespace EventRegistrar.Backend.Authentication;

public class Auth0Configuration : IConfigurationItem
{
    public string ClientIdKey { get; set; } = null!;
    public string ClientSecretKey { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string TokenUrl { get; set; } = null!;
}

public class DefaultAuth0Configuration : Auth0Configuration, IDefaultConfigurationItem
{
    public DefaultAuth0Configuration()
    {
        ClientIdKey = "Auth0-ClientId";
        ClientSecretKey = "Auth0-ClientSecret";
        Audience = "https://eventregistrar.eu.auth0.com/api/v2/";
        TokenUrl = "https://eventregistrar.eu.auth0.com/oauth/token";
    }
}