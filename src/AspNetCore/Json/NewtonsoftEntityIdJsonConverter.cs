using System.Reflection;
using NetCorePal.Extensions.Domain;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.AspNetCore.Json;

public class NewtonsoftEntityIdJsonConverter : JsonConverter
{
    readonly Dictionary<Type, ConstructorInfo> _ConstructorInfo = new();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
            writer.WriteNull();
        else if (value is IInt32StronglyTypedId int32Id)
            writer.WriteValue(int32Id.Id);
        else
            writer.WriteValue(value.ToString());
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        object? v = default;

        var value = reader.Value?.ToString();

        if (value != null)
        {
            if (!_ConstructorInfo.TryGetValue(objectType, out var _constructorInfo))
            {
                _constructorInfo = objectType.GetConstructors()[0];
                _ConstructorInfo.Add(objectType, _constructorInfo);
            }

            Type parameterType = _constructorInfo.GetParameters()[0].ParameterType;

            if (parameterType == typeof(long))
            {
                v = _constructorInfo.Invoke(new object[] { long.Parse(value) });
            }
            else if (parameterType == typeof(int))
            {
                v = _constructorInfo.Invoke(new object[] { int.Parse(value) });
            }
            else if (parameterType == typeof(string))
            {
                v = _constructorInfo.Invoke(new object[] { value });
            }
            else if (parameterType == typeof(Guid))
            {
                v = _constructorInfo.Invoke(new object[] { Guid.Parse(value) });
            }
        }


        return v;
    }

    readonly Type[] _supportedTypes = new[]
    {
        typeof(IInt64StronglyTypedId),
        typeof(IInt32StronglyTypedId),
        typeof(IGuidStronglyTypedId),
        typeof(IStringStronglyTypedId),
    };

    public override bool CanConvert(Type objectType)
    {
        return Array.Exists(_supportedTypes, p => p.IsAssignableFrom(objectType));
    }
}