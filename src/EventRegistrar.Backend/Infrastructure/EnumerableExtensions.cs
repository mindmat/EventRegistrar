namespace EventRegistrar.Backend.Infrastructure;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> FillUpIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource> createFillElement, int minLength)
    {
        return condition
            ? FillUp(source, createFillElement, minLength)
            : source;
    }

    public static IEnumerable<TSource> FillUp<TSource>(this IEnumerable<TSource> source, Func<TSource> createFillElement, int minLength)
    {
        var list = source.ToList();
        IEnumerable<TSource> filledList = list;
        var count = list.Count;
        for (var i = count; i < minLength; i++)
            filledList = filledList.Append(createFillElement());

        return filledList;
    }
}