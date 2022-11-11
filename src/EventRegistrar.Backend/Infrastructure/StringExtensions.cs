namespace EventRegistrar.Backend.Infrastructure;

public static class StringExtensions
{
    public static string StringJoin(this IEnumerable<string> strings, string separator = ", ")
    {
        return string.Join(separator, strings);
    }

    public static decimal? TryToDecimal(this string? text)
    {
        return text != null && decimal.TryParse(text, out var number)
                   ? number
                   : null;
    }
}