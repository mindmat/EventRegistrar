namespace EventRegistrar.Backend.Infrastructure;

public static class StringExtensions
{
    public const char KeySeparator = (char)31; // unit separator(), https://ronaldduncan.wordpress.com/2009/10/31/text-file-formats-ascii-delimited-text-not-csv-or-tab-delimited-text/

    public static string StringJoin(this IEnumerable<string> strings, string separator = ", ")
    {
        return string.Join(separator, strings);
    }

    public static string StringJoinNullable(this IEnumerable<string?> strings, string separator = ", ")
    {
        return string.Join(separator, strings.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public static decimal? TryToDecimal(this string? text)
    {
        return text != null && decimal.TryParse(text, out var number)
                   ? number
                   : null;
    }

    /// <summary>
    /// [a,b,c] -> "a{us}b{us}c"
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static string? MergeKeys(this IEnumerable<string>? items)
    {
        return items?.Any() != true
                   ? null
                   : string.Join(KeySeparator, items);
    }

    /// <summary>
    /// [a,b,c] -> "a{us}b{us}c"
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static string? MergeKeys(this IEnumerable<Guid>? items)
    {
        return items?.Select(id => id.ToString())
                    .MergeKeys();
    }

    /// <summary>
    /// "a{us}b{us}c" -> [a,b,c]
    /// </summary>
    /// <param name="mergedKeys"></param>
    /// <returns></returns>
    public static IEnumerable<string> SplitKeys(this string? mergedKeys)
    {
        return mergedKeys?.Split(KeySeparator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
    }
}