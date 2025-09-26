using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCorePal.Extensions.Domain.Json;

public class RowVersionJsonConverter : JsonConverter<RowVersion>
{
    public override RowVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is JsonTokenType.Number && reader.TryGetInt32(out var value))
        {
            return new RowVersion(value);
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, RowVersion value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.VersionNumber);
    }
}