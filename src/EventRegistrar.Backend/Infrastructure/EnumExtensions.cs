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
}