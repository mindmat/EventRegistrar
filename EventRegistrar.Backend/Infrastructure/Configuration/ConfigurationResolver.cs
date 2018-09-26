using System;
using System.Collections.Generic;
using System.Linq;
using EventRegistrar.Backend.Events.Context;
using Newtonsoft.Json;

namespace EventRegistrar.Backend.Infrastructure.Configuration
{
    public class ConfigurationResolver
    {
        private readonly IQueryable<EventConfiguration> _configurations;
        private readonly IEnumerable<IDefaultConfigurationItem> _defaultConfigurations;
        private readonly EventContext _eventContext;

        public ConfigurationResolver(IEnumerable<IDefaultConfigurationItem> defaultConfigurations,
                                     IQueryable<EventConfiguration> configurations,
                                     EventContext eventContext)
        {
            _defaultConfigurations = defaultConfigurations;
            _configurations = configurations;
            _eventContext = eventContext;
        }

        public T GetConfiguration<T>()
           where T : class, IConfigurationItem
        {
            if (_eventContext.EventId != null)
            {
                var dbConfig = _configurations.FirstOrDefault(cfg => cfg.EventId == _eventContext.EventId
                                                                     && cfg.Type == typeof(T).FullName);
                if (dbConfig != null)
                {
                    return JsonConvert.DeserializeObject<T>(dbConfig.ValueJson);
                }
            }

            var defaultConfig = _defaultConfigurations
                                .FirstOrDefault(dfc => dfc.GetType().BaseType == typeof(T));
            return defaultConfig as T;
        }

        public IConfigurationItem GetConfigurationTypeless(Type type)
        {
            return typeof(ConfigurationResolver).GetMethod(nameof(GetConfiguration))
                                                .MakeGenericMethod(type)
                                                .Invoke(this, new object[] { }) as IConfigurationItem;
        }
    }
}