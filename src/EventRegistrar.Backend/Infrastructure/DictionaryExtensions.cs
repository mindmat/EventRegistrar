using System.Collections.Generic;

namespace EventRegistrar.Backend.Infrastructure
{
    public static class DictionaryExtensions
    {
        public static TValue? Lookup<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey? key)
            where TKey : struct
            where TValue : struct
        {
            if (key != null && dictionary.TryGetValue(key.Value, out var value))
            {
                return value;
            }
            return null;
        }
        public static TValue? Lookup<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : struct
            where TValue : struct
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
