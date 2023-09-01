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
            return typeToConvert.GetInterfaces().Any(p => p == typeof(IStronglyTypedId<>));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return Cache.GetOrAdd(typeToConvert, CreateConverter);
        }

        private JsonConverter CreateConverter(Type typeToConvert)
        {
            var stronglyTypedIdTypeInterfaces =
                typeToConvert.GetInterfaces().Where(p => p == typeof(IStronglyTypedId<>));
            foreach (var stronglyTypedIdTypeInterface in stronglyTypedIdTypeInterfaces)
            {
                var type = typeof(EntityIdJsonConverter<,>).MakeGenericType(typeToConvert,
                    stronglyTypedIdTypeInterface.GetGenericArguments().First());
                var v = Activator.CreateInstance(type);
                if (v == null)
                {
                    throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");
                }

                return (JsonConverter)v;
            }

            throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");
        }
    }
}