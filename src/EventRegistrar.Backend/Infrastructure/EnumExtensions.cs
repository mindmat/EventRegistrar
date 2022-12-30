using System.Reflection;

namespace EventRegistrar.Backend.Infrastructure;

public static class EnumExtensions
{
    public static bool HasAttribute<TAttribute>(this Enum value)
        where TAttribute : Attribute
    {
        return value.GetAttribute<TAttribute>() != null;
    }

    public static TAttribute? GetAttribute<TAttribute>(this Enum value)
        where TAttribute : Attribute
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        return name == null
                   ? null
                   : type.GetField(name)?.GetCustomAttribute<TAttribute>();
    }

    public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Where(e => value.HasFlag(e));
    }

    public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum? value)
        where TEnum : struct, Enum
    {
        return value == null
                   ? Enumerable.Empty<TEnum>()
                   : GetFlags(value.Value);
    }

    public static TEnum? ConvertToFlags<TEnum>(this IEnumerable<TEnum>? values)
        where TEnum : struct, Enum
    {
        if (values == null || !values.Any())
        {
            return null;
        }

        dynamic result = values.First();

        foreach (var value in values)
        {
            result |= value;
        }

        return (TEnum)result;
    }
}