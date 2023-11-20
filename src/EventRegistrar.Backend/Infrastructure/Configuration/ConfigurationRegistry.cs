using EventRegistrar.Backend.Events.Context;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.Configuration;

public class ConfigurationRegistry(IEnumerable<IDefaultConfigurationItem> defaultConfigurations,
                                   IRepository<EventConfiguration> configurations,
                                   EventContext eventContext)
{
    public T GetConfiguration<T>(Guid? eventId = null)
        where T : class, IConfigurationItem
    {
        eventId ??= eventContext.EventId;
        if (eventId != null)
        {
            var dbConfig = configurations.FirstOrDefault(cfg => cfg.EventId == eventId.Value
                                                             && cfg.Type == typeof(T).FullName);
            if (dbConfig != null)
            {
                return JsonConvert.DeserializeObject<T>(dbConfig.ValueJson);
            }
        }

        var defaultConfig = defaultConfigurations
            .FirstOrDefault(dfc => dfc.GetType().BaseType == typeof(T));
        return defaultConfig as T;
    }

    public async Task UpdateConfiguration<T>(Guid eventId, T newConfig)
        where T : class, IConfigurationItem
    {
        var dbConfig = await configurations.AsTracking()
                                           .FirstOrDefaultAsync(cfg => cfg.EventId == eventId
                                                                    && cfg.Type == typeof(T).FullName)
                    ?? configurations.InsertObjectTree(new EventConfiguration
                                                       {
                                                           Id = Guid.NewGuid(),
                                                           EventId = eventId,
                                                           Type = typeof(T).FullName!
                                                       });
        dbConfig.ValueJson = JsonConvert.SerializeObject(newConfig);
    }

    public IConfigurationItem GetConfigurationTypeless(Type type)
    {
        return typeof(ConfigurationRegistry).GetMethod(nameof(GetConfiguration))
                                            .MakeGenericMethod(type)
                                            .Invoke(this, new object[] { null }) as IConfigurationItem;
    }
}