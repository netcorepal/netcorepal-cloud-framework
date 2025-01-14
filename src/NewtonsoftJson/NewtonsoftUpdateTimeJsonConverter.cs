using NetCorePal.Extensions.Domain;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.NewtonsoftJson;

public class NewtonsoftUpdateTimeJsonConverter : JsonConverter<UpdateTime>
{
    public override void WriteJson(JsonWriter writer, UpdateTime? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.Value);
        }
    }

    public override UpdateTime? ReadJson(JsonReader reader, Type objectType, UpdateTime? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is null)
        {
            return null;
        }

        if (reader.Value is DateTimeOffset timeOffset)
        {
            return new UpdateTime(timeOffset);
        }

        if (reader.Value is DateTime dateTime)
        {
            return new UpdateTime(dateTime);
        }

        if (reader.Value is string str)
        {
            return DateTimeOffset.TryParse(str, out var dateTimeOffset) ? new UpdateTime(dateTimeOffset) : null;
        }

        return null;
    }
}