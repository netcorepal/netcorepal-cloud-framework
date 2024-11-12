using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace NetCorePal.Extensions.Domain
{
    public class EntityIdTypeConverter<TStronglyTypedId, TSource> : TypeConverter
        where TStronglyTypedId : IStronglyTypedId<TSource>
    {
        private readonly ConstructorInfo _constructorInfo;

        public EntityIdTypeConverter()
        {
            var entityIdType = typeof(TStronglyTypedId);
            var constructorInfo = Array.Find(entityIdType.GetConstructors(), c =>
                                      c.GetParameters().Length == 1 &&
                                      c.GetParameters()[0].ParameterType == typeof(TSource))
                                  ?? throw new Exception($"类型 {entityIdType}必须有一个仅包含{typeof(TSource).Name}类型作为参数的构造函数");
            _constructorInfo = constructorInfo;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || sourceType == typeof(TSource);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || destinationType == typeof(TSource);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string strValue)
            {
                if (typeof(TSource) == typeof(long))
                    return _constructorInfo.Invoke(new object[] { long.Parse(strValue) });
                if (typeof(TSource) == typeof(int))
                    return _constructorInfo.Invoke(new object[] { int.Parse(strValue) });
                if (typeof(TSource) == typeof(string))
                    return _constructorInfo.Invoke(new object[] { strValue });
                if (typeof(TSource) == typeof(Guid))
                    return _constructorInfo.Invoke(new object[] { Guid.Parse(strValue) });
            }
            else if (value is long longValue && typeof(TSource) == typeof(long))
            {
                return _constructorInfo.Invoke(new object[] { longValue });
            }
            else if (value is int intValue && typeof(TSource) == typeof(int))
            {
                return _constructorInfo.Invoke(new object[] { intValue });
            }
            else if (value is string stringValue && typeof(TSource) == typeof(string))
            {
                return _constructorInfo.Invoke(new object[] { stringValue });
            }
            else if (value is Guid guidValue && typeof(TSource) == typeof(Guid))
            {
                return _constructorInfo.Invoke(new object[] { guidValue });
            }

#pragma warning disable S112
            throw new Exception($"无法从{value.GetType()} 转换为 {context?.PropertyDescriptor?.PropertyType}");
#pragma warning restore S112
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string? strValue = value?.ToString();

                if (strValue != null)
                {
                    return strValue;
                }
                else
                {
                    return value;
                }
            }
            else if (destinationType == typeof(Int32) && value is IInt32StronglyTypedId int32Value)
            {
                return int32Value.Id;
            }
            else if (destinationType == typeof(Int64) && value is IInt64StronglyTypedId int64Value)
            {
                return int64Value.Id;
            }
            else if (destinationType == typeof(Guid) && value is IGuidStronglyTypedId guidValue)
            {
                return guidValue.Id;
            }

            throw new ArgumentException($"Cannot convert {value ?? "(null)"} to {destinationType}",
                nameof(destinationType));
        }
    }
}