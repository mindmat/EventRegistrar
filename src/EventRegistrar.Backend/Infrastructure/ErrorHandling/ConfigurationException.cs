namespace EventRegistrar.Backend.Infrastructure.ErrorHandling;

public class ConfigurationException : ApplicationException
{
    public ConfigurationException(string configurationKey)
    {
        ConfigurationKey = configurationKey;
    }

    public string ConfigurationKey { get; }
}