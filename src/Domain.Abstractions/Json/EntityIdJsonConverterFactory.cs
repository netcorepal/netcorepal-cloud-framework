using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Domain.Json
{
    public class EntityIdJsonConverterFactory : JsonConverterFactory
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.GetInterfaces().Any(p => p == typeof(IEntityId));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return Cache.GetOrAdd(typeToConvert, CreateConverter);
        }

        private static JsonConverter CreateConverter(Type typeToConvert)
        {
            if (!typeToConvert.GetInterfaces().Any(p => p == typeof(IEntityId)))
                throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");

            var type = typeof(EntityIdJsonConverter<>).MakeGenericType(typeToConvert);

            if (type == null)
            {
                throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");
            }

            var v = Activator.CreateInstance(type);
            if (v == null)
            {
                throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");
            }
            return (JsonConverter)v;
        }
    }
}
