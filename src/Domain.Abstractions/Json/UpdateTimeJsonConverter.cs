using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCorePal.Extensions.Domain.Json;

public class UpdateTimeJsonConverter : JsonConverter<UpdateTime>
{
    public override UpdateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is JsonTokenType.String)
        {
            var value = reader.GetString();
            if (value != null)
            {
                return new UpdateTime(DateTimeOffset.Parse(value));
            }
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, UpdateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}