using System.Collections.Generic;

namespace EventRegistrator.Functions.Infrastructure
{
    public static class DictionaryExtensionMethods
    {
        public static TValue Lookup<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue fallbackValue = default(TValue))
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return fallbackValue;
        }
    }
}