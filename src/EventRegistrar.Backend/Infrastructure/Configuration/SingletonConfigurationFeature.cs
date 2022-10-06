namespace EventRegistrar.Backend.Infrastructure.Configuration;

public class SingletonConfigurationFeature<TFeatureConfiguration>
    where TFeatureConfiguration : IConfigurationItem
{
    public SingletonConfigurationFeature(TFeatureConfiguration configuration)
    {
        Configuration = configuration;
    }

    public TFeatureConfiguration Configuration { get; }
}