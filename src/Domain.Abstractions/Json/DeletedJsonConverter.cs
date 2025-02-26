using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCorePal.Extensions.Domain.Json;

public class DeletedJsonConverter : JsonConverter<Deleted>
{
    public override Deleted? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.String:
                {
                    var stringValue = reader.GetString();
                    if (bool.TryParse(stringValue, out var boolValue)) return new Deleted(boolValue);
                    throw new JsonException($"无效的布尔字符串值: '{stringValue}'");
                }

                case JsonTokenType.True:
                    return new Deleted(true);
                case JsonTokenType.False:
                    return new Deleted();

                case JsonTokenType.Number:
                case JsonTokenType.None:
                case JsonTokenType.StartObject:
                case JsonTokenType.EndObject:
                case JsonTokenType.StartArray:
                case JsonTokenType.EndArray:
                case JsonTokenType.PropertyName:
                case JsonTokenType.Comment:
                default:
                    throw new JsonException($"意外的Token类型 {reader.TokenType}（位置：{reader.Position}），预期布尔值或字符串");
            }
        }
        catch (Exception ex)
        {
            throw new JsonException($"反序列化 Deleted 失败: {ex.Message}", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, Deleted value, JsonSerializerOptions options)
    {
        try
        {
            writer.WriteBooleanValue(value.Value);
        }
        catch (Exception ex)
        {
            throw new JsonException($"序列化 Deleted 失败: {ex.Message}", ex);
        }
    }
}