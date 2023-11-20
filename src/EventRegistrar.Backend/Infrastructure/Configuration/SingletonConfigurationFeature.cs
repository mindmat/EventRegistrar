namespace EventRegistrar.Backend.Infrastructure.Configuration;

public class SingletonConfigurationFeature<TFeatureConfiguration>(TFeatureConfiguration configuration)
    where TFeatureConfiguration : IConfigurationItem
{
    public TFeatureConfiguration Configuration { get; } = configuration;
}