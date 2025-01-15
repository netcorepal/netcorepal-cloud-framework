using NetCorePal.Extensions.Domain;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.NewtonsoftJson;

public class NewtonsoftRowVersionJsonConverter : JsonConverter<RowVersion>
{
    public override void WriteJson(JsonWriter writer, RowVersion? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.VersionNumber);
        }
    }

    public override RowVersion? ReadJson(JsonReader reader, Type objectType, RowVersion? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is null)
        {
            return null;
        }

        if (reader.Value is Int64 longValue)
        {
            return new RowVersion((int)longValue);
        }

        if (reader.Value is Int32 value)
        {
            return new RowVersion(value);
        }


        if (reader.Value is string str)
        {
            return Int32.TryParse(str, out var v) ? new RowVersion(v) : null;
        }

        return null;
    }
}