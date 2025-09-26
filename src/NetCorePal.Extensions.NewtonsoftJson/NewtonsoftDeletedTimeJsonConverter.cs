using NetCorePal.Extensions.Domain;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.NewtonsoftJson;

public class NewtonsoftDeletedTimeJsonConverter : JsonConverter<DeletedTime>
{
    public override void WriteJson(JsonWriter writer, DeletedTime? value, JsonSerializer serializer)
    {
        try
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.Value);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"序列化DeletedTime失败（路径：{writer.Path}）", ex);
        }
    }

    public override DeletedTime? ReadJson(
        JsonReader reader,
        Type objectType,
        DeletedTime? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        try
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;

                case JsonToken.String:
                {
                    var stringValue = (string)reader.Value!;

                    if (DateTimeOffset.TryParse(stringValue, out var dateTimeOffset))
                        return new DeletedTime(dateTimeOffset);

                    throw new JsonSerializationException($"无效的日期格式 '{stringValue}'（路径：{reader.Path}）");
                }

                case JsonToken.Date:
                {
                    // 处理原生 DateTime 类型（自动转换时区）
                    var date = (DateTime)reader.Value!;
                    var dto = date.Kind switch
                    {
                        DateTimeKind.Utc => new DateTimeOffset(date, TimeSpan.Zero),
                        DateTimeKind.Local => new DateTimeOffset(date.ToUniversalTime(), TimeSpan.Zero),
                        _ => new DateTimeOffset(date, TimeSpan.Zero) // 未指定时区按本地时间处理
                    };
                    return new DeletedTime(dto);
                }

                case JsonToken.Integer:
                case JsonToken.None:
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                case JsonToken.StartConstructor:
                case JsonToken.PropertyName:
                case JsonToken.Comment:
                case JsonToken.Raw:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                case JsonToken.EndConstructor:
                case JsonToken.Bytes:
                default:
                    throw new JsonSerializationException(
                        $"意外的Token类型 {reader.TokenType}（路径：{reader.Path}），预期ISO 8601日期字符串、 日期对象（DateTime）或null");
            }
        }
        catch (JsonSerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"反序列化DeletedTime失败（路径：{reader.Path}）", ex);
        }
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}