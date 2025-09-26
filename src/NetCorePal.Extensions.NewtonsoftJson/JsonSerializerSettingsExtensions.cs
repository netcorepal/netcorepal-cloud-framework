using NetCorePal.Extensions.NewtonsoftJson;

namespace Newtonsoft.Json;

public static class JsonSerializerSettingsExtensions
{
    public static JsonSerializerSettings AddNetCorePalJsonConverters(this JsonSerializerSettings settings)
    {
        settings.Converters.Add(new NewtonsoftEntityIdJsonConverter());
        settings.Converters.Add(new NewtonsoftUpdateTimeJsonConverter());
        settings.Converters.Add(new NewtonsoftRowVersionJsonConverter());
        settings.Converters.Add(new NewtonsoftDeletedJsonConverter());
        settings.Converters.Add(new NewtonsoftDeletedTimeJsonConverter());
        return settings;
    }
}