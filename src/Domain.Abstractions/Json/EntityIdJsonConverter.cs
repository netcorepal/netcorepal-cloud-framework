using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Domain.Json
{
    public class EntityIdJsonConverter<TEntityId> : JsonConverter<TEntityId>
    where TEntityId : IEntityId
    {

        private EntityIdTypeConverter typeConverter = new EntityIdTypeConverter(typeof(TEntityId));


        public override TEntityId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.Null)
                return default(TEntityId);

            var value = reader.GetString();
            if (value != null)
            {
                var v = typeConverter.ConvertFrom(value);
                if (v != null)
                {
                    return (TEntityId)v;
                }
            }
            return default(TEntityId);
        }

        public override void Write(Utf8JsonWriter writer, TEntityId value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.ToString());
        }
    }
}
