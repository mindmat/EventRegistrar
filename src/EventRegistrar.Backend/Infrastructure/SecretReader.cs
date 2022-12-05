using System.Diagnostics;

using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using EventRegistrar.Backend.Infrastructure.ErrorHandling;

namespace EventRegistrar.Backend.Infrastructure;

public class SecretReader
{
    private const string _keyVaultConfigKey = "KeyVaultUri";
    private readonly ILogger _logger;
    private readonly Lazy<SecretClient?> _secretClient;

    public SecretReader(IConfiguration configuration,
                        ILogger logger)
    {
        _logger = logger;
        _secretClient = new Lazy<SecretClient?>(() => CreateSecretClient(configuration));
    }

    public async Task<string?> GetSecret(string key, CancellationToken cancellationToken = default)
    {
        if (_secretClient.Value == null)
        {
            return null;
        }

        var response = await _secretClient.Value.GetSecretAsync(key, null, cancellationToken);
        return response.Value.Value;
    }

    private SecretClient CreateSecretClient(IConfiguration configuration)
    {
        var keyVaultUri = configuration.GetValue<string>(_keyVaultConfigKey)
                       ?? throw new ConfigurationException(_keyVaultConfigKey);

        TokenCredential credential = Debugger.IsAttached
                                         ? new InteractiveBrowserCredential() // ManagedIdentityCredential doesn't work for local dev 
                                         : new ManagedIdentityCredential();
        var client = new SecretClient(new Uri(keyVaultUri), credential);

        try
        {
            var _ = client.GetPropertiesOfSecrets().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Could not create SecretClient. Credential type {credential.GetType().Name}");
        }

        return client;
    }
}