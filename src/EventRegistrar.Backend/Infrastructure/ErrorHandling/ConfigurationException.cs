namespace EventRegistrar.Backend.Infrastructure.ErrorHandling;

public class ConfigurationException(string configurationKey) : ApplicationException
{
    public string ConfigurationKey { get; } = configurationKey;
}