using System.Collections;
using System.Reflection;

using EventRegistrar.Backend.Infrastructure;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Newtonsoft.Json;

namespace EventRegistrar.Backend.RegistrationForms.FormPaths;

public static class StorageConverters
{
    private static readonly JsonSerializerSettings Settings = new()
                                                              {
                                                                  DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                  TypeNameHandling = TypeNameHandling.Auto
                                                              };

    public static ValueConverter<T?, string?> JsonConverter<T>()
        where T : class
    {
        return new ValueConverter<T?, string?>(
            value => value == null ? null : JsonConvert.SerializeObject(value, Settings),
            json => json == null ? null : JsonConvert.DeserializeObject<T>(json, Settings));
    }

    public static PropertyBuilder IsJsonColumn<T>(this PropertyBuilder<T?> builder)
        where T : class
    {
        const string jsonColumnSuffix = "Json";
        var converter = new ValueConverter<T?, string?>(
            items => items == null ? null : JsonConvert.SerializeObject(items),
            json => json == null ? null : JsonConvert.DeserializeObject<T?>(json));

        builder = builder.HasConversion(converter);
        var columnName = builder.Metadata.Name;
        if (!columnName.EndsWith(jsonColumnSuffix))
        {
            builder = builder.HasColumnName($"{columnName}{jsonColumnSuffix}");
        }

        return builder;
    }

    public static PropertyBuilder IsKeysColumn(this PropertyBuilder<ICollection<Guid>?> config)
    {
        return config.HasConversion(new ValueConverter<ICollection<Guid>?, string>(guids => guids != null && guids.Count > 0
                                                                                                ? guids.MergeKeys()!
                                                                                                : string.Empty,
                                                                                   csv => csv.SplitGuidKeys().ToList()));
    }

    private const string CommaSeparator = ",";

    public static PropertyBuilder IsCsvColumn(this PropertyBuilder<ICollection<Guid>> config)
    {
        return config.HasConversion(new ValueConverter<ICollection<Guid>, string?>(guids => guids.Count > 0
                                                                                                ? string.Join(CommaSeparator, guids)
                                                                                                : null,
                                                                                   csv => ParseCsvGuids(csv)));
    }

    public static ICollection<Guid> ParseCsvGuids(string? csv)
    {
        return csv?.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                  .Select(Guid.Parse)
                  .ToList()
            ?? new List<Guid>(0);
    }


    public static PropertyBuilder IsCsvColumn<T>(this PropertyBuilder<ICollection<T>?> config)
        where T : struct, Enum
    {
        return config.HasConversion(new ValueConverter<ICollection<T>?, string?>(enums => enums == null ? null : string.Join(CommaSeparator, enums),
                                                                                 csv => ParseCsvEnums<T>(csv)));
    }

    public static ICollection<T>? ParseCsvEnums<T>(string? csv) where T : struct, Enum
    {
        return csv?.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                  .Select(Enum.Parse<T>)
                  .ToArray();
    }

    private static readonly MethodInfo _parseCsvEnumsMethod = typeof(StorageConverters).GetMethod(nameof(ParseCsvEnums))
                                                           ?? throw new NotImplementedException("ParseCsvEnums");


    public static IEnumerable? ParseCsvEnumsTypeless(Type enumType, string? csv)
    {
        var method = _parseCsvEnumsMethod.MakeGenericMethod(enumType);
        return method.Invoke(null, new object?[] { csv }) as IEnumerable;
    }
}