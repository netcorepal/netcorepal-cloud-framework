using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCorePal.Extensions.Domain.Json;

public class DeletedTimeJsonConverter : JsonConverter<DeletedTime>
{
    public override DeletedTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.String:
                {
                    var value = reader.GetString();
                    if (string.IsNullOrEmpty(value))
                        return null;

                    if (DateTimeOffset.TryParse(value, out var dateTimeOffset)) return new DeletedTime(dateTimeOffset);

                    throw new JsonException($"无效的日期时间格式: '{value}' (位置：{reader.Position})");
                }

                case JsonTokenType.Number:
                case JsonTokenType.None:
                case JsonTokenType.StartObject:
                case JsonTokenType.EndObject:
                case JsonTokenType.StartArray:
                case JsonTokenType.EndArray:
                case JsonTokenType.PropertyName:
                case JsonTokenType.Comment:
                case JsonTokenType.True:
                case JsonTokenType.False:
                default:
                    throw new JsonException(
                        $"意外的Token类型 {reader.TokenType} (位置：{reader.Position})，预期ISO 8601日期字符串或null");
            }
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException($"反序列化DeletedTime失败: {ex.Message}", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, DeletedTime value, JsonSerializerOptions options)
    {
        try
        {
            writer.WriteStringValue(value.Value);
        }
        catch (Exception ex)
        {
            throw new JsonException($"序列化DeletedTime失败: {ex.Message}", ex);
        }
    }
}