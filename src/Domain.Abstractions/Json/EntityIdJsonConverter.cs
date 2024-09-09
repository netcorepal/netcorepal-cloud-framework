using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Domain.Json
{
    public class EntityIdJsonConverter<TEntityId, TSource> : JsonConverter<TEntityId>
        where TEntityId : IStronglyTypedId<TSource>
    {
        readonly EntityIdTypeConverter<TEntityId, TSource> _typeConverter = new();

        public override TEntityId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.Null)
                return default;

            if (reader.TokenType is JsonTokenType.String)
            {
                var value = reader.GetString();
                if (value != null)
                {
                    var v = _typeConverter.ConvertFrom(value);
                    if (v != null)
                    {
                        return (TEntityId)v;
                    }
                }
            }
            else if (reader.TokenType is JsonTokenType.Number)
            {
                if (typeof(TSource) == typeof(long))
                {
                    var int64Id = _typeConverter.ConvertFrom(reader.GetInt64());
                    if (int64Id != null)
                    {
                        return (TEntityId)int64Id;
                    }
                }

                if (typeof(TSource) == typeof(int))
                {
                    var int32Id = _typeConverter.ConvertFrom(reader.GetInt32());
                    if (int32Id != null)
                    {
                        return (TEntityId)int32Id;
                    }
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, TEntityId? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else if (value is IInt32StronglyTypedId int32Value)
                writer.WriteNumberValue(int32Value.Id);
            else
                writer.WriteStringValue(value.ToString());
        }
    }
}