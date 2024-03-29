﻿using EventRegistrar.Backend.Properties;

namespace EventRegistrar.Backend.Infrastructure;

public class EnumTranslator
{
    public string Translate<TEnum>(TEnum value)
        where TEnum : struct
    {
        var key = $"{typeof(TEnum).Name}_{value}";
        return Resources.ResourceManager.GetString(key) ?? value.ToString() ?? "??";
    }

    public string? Translate<TEnum>(TEnum? value)
        where TEnum : struct
    {
        return value == null
                   ? null
                   : Translate(value.Value);
    }

    public Dictionary<TEnum, string> TranslateAll<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().ToDictionary(env => env, Translate);
    }
}