using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public static class JsonConverter
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public static ValueConverter<T, string> Converter<T>()
    {
        return new ValueConverter<T, string>(
            item => JsonSerializer.Serialize(item, _options),
            json => JsonSerializer.Deserialize<T>(json, _options)!);
    }
}