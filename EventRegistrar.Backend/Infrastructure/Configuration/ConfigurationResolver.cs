using System;
using System.Collections.Generic;
using System.Linq;

namespace EventRegistrar.Backend.Infrastructure.Configuration
{
    public class ConfigurationResolver
    {
        private readonly IEnumerable<IDefaultConfigurationItem> _defaultConfigurations;

        public ConfigurationResolver(IEnumerable<IDefaultConfigurationItem> defaultConfigurations)
        {
            _defaultConfigurations = defaultConfigurations;
        }

        public T GetConfiguration<T>()
           where T : class, IConfigurationItem
        {
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