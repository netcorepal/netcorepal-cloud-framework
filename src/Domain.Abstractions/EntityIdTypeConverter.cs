using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;

namespace NetCorePal.Extensions.Domain
{
    public class EntityIdTypeConverter<TStronglyTypedId, TSource> : TypeConverter
        where TStronglyTypedId : IStronglyTypedId<TSource>
    {
        private readonly Type _entityIdType;
        private readonly ConstructorInfo _constructorInfo;

        public EntityIdTypeConverter()
        {
            _entityIdType = typeof(TStronglyTypedId);
            var constructorInfo = _entityIdType.GetConstructors().Where(c =>
                    c.GetParameters().Count() == 1 && c.GetParameters().First().ParameterType == typeof(TSource))
                .FirstOrDefault();
            if (constructorInfo == null)
            {
                throw new Exception($"类型 {_entityIdType}必须有一个仅包含{typeof(TSource).Name}类型作为参数的构造函数");
            }

            _constructorInfo = constructorInfo;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(string) || sourceType == typeof(TSource);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
            destinationType == typeof(string) || destinationType == typeof(TSource);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
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

            throw new Exception($"无法从{value.GetType()} 转换为 {context.PropertyDescriptor?.PropertyType}");
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string? strValue = value.ToString();

                if (strValue != null)
                {
                    return strValue;
                }
                else
                {
                    return value;
                }
            }

            throw new ArgumentException($"Cannot convert {value ?? "(null)"} to {destinationType}",
                nameof(destinationType));
        }
    }
}