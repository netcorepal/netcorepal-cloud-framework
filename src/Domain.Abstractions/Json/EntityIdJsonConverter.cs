using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Domain.Json
{
    public class EntityIdJsonConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {

            return typeof(IEntityId).IsAssignableFrom(typeToConvert);
            //return typeToConvert.IsAssignableFrom(typeof(EntityId));
            //return base.CanConvert(typeToConvert);
        }

        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Activator.CreateInstance(typeToConvert, long.Parse(value));
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
