using NetCorePal.Extensions.Domain;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.NewtonsoftJson;

public class NewtonsoftDeletedJsonConverter : JsonConverter<Deleted>
{
    public override void WriteJson(JsonWriter writer, Deleted? value, JsonSerializer serializer)
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
            throw new JsonSerializationException($"序列化 Deleted 失败（路径：{writer.Path}）", ex);
        }
    }

    public override Deleted? ReadJson(
        JsonReader reader,
        Type objectType,
        Deleted? existingValue,
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
                    if (bool.TryParse(stringValue, out var boolValue)) return new Deleted(boolValue);
                    throw new JsonSerializationException($"无效的布尔字符串值 '{stringValue}'（路径：{reader.Path}）");
                }

                case JsonToken.Boolean:
                    return new Deleted((bool)reader.Value!);

                case JsonToken.Integer:
                case JsonToken.None:
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                case JsonToken.StartConstructor:
                case JsonToken.PropertyName:
                case JsonToken.Comment:
                case JsonToken.Raw:
                case JsonToken.Float:
                case JsonToken.Undefined:
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                case JsonToken.EndConstructor:
                case JsonToken.Date:
                case JsonToken.Bytes:
                default:
                    throw new JsonSerializationException($"意外的Token类型 {reader.TokenType}（路径：{reader.Path}），预期布尔值或字符串");
            }
        }
        catch (JsonSerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"反序列化 Deleted 失败（路径：{reader.Path}）", ex);
        }
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}