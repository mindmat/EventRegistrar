namespace EventRegistrar.Backend.Infrastructure;

public static class DictionaryExtensions
{
    public static TValue? Lookup<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey? key)
        where TKey : struct
        where TValue : struct
    {
        return key != null && dictionary.TryGetValue(key.Value, out var value)
                   ? value
                   : null;
    }

    public static TValue? Lookup<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : struct
        where TValue : struct
    {
        return dictionary.TryGetValue(key, out var value)
                   ? value
                   : null;
    }

    public static TValue? LookupNullable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : struct
    {
        return dictionary.TryGetValue(key, out var value)
                   ? value
                   : null;
    }
}