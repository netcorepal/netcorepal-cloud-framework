using NetCorePal.Extensions.Domain.Json;

namespace System.Text.Json;

public static class JsonSerializerOptionsExtensions
{
    public static void AddNetCorePalJsonConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new EntityIdJsonConverterFactory());
        options.Converters.Add(new UpdateTimeJsonConverter());
    }
}