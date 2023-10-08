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

            var value = reader.GetString();
            if (value != null)
            {
                var v = _typeConverter.ConvertFrom(value);
                if (v != null)
                {
                    return (TEntityId)v;
                }
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, TEntityId? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.ToString());
        }
    }
}