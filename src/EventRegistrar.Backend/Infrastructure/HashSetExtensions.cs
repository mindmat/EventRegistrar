namespace EventRegistrar.Backend.Infrastructure;

public static class HashSetExtensions
{
    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> newItems)
    {
        foreach (var newItem in newItems)
        {
            hashSet.Add(newItem);
        }
    }
}