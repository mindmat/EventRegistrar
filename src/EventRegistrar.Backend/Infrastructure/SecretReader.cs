using System.Collections.Concurrent;
using System.Security;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using EventRegistrar.Backend.Infrastructure.ErrorHandling;

namespace EventRegistrar.Backend.Infrastructure;

public class SecretReader
{
    private const string _keyVaultConfigKey = "KeyVaultUri";
    private readonly ILogger _logger;
    private readonly Lazy<SecretClient> _secretClient;
    private readonly IDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

    public SecretReader(IConfiguration configuration,
                        ILogger logger)
    {
        _logger = logger;
        _secretClient = new Lazy<SecretClient>(() => CreateSecretClient(configuration));
    }

    public async Task<string?> GetSecret(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var cachedSecret))
        {
            return cachedSecret;
        }

        var response = await _secretClient.Value.GetSecretAsync(key, null, cancellationToken);
        var secret = response.Value.Value;
        _cache[key] = secret;
        return secret;
    }

    public Task<string?> GetSendGridApiKey(CancellationToken cancellationToken = default)
    {
        const string SendGridApiKey = "SendGridApiKey";
        return GetSecret(SendGridApiKey, cancellationToken);
    }

    public Task<string?> GetPostmarkToken(CancellationToken cancellationToken = default)
    {
        const string PostmarkTokenKey = "PostmarkToken";
        return GetSecret(PostmarkTokenKey, cancellationToken);
    }

    private SecretClient CreateSecretClient(IConfiguration configuration)
    {
        var keyVaultUri = configuration.GetValue<string>(_keyVaultConfigKey)
                       ?? throw new ConfigurationException(_keyVaultConfigKey);

        var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

        try
        {
            _ = client.GetPropertiesOfSecrets().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not create SecretClient.");
        }

        return client;
    }
}