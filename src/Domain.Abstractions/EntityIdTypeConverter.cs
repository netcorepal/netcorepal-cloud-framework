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
    public class EntityIdTypeConverter : TypeConverter
    {

        static ConcurrentDictionary<Type, Func<string, object>> _constructorInfoCache = new ConcurrentDictionary<Type, Func<string, object>>();


        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
             sourceType == typeof(string);
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
            destinationType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string strValue)
            {
                var type = context.PropertyDescriptor.PropertyType;


                var func = _constructorInfoCache.GetOrAdd(type, (t) =>
                {
                    var constructors = type.GetConstructors();

                    var list = constructors.Where(c => c.GetParameters().Count() == 1);

                    var guidConstructor = list.FirstOrDefault(c => c.GetParameters().First().ParameterType == typeof(Guid));

                    if (guidConstructor != null)
                    {
                        return str => guidConstructor.Invoke(new object[] { Guid.Parse(str) });
                    }
                    var longConstructor = list.FirstOrDefault(c => c.GetParameters().First().ParameterType == typeof(long));
                    if (longConstructor != null)
                    {

                        return str => longConstructor.Invoke(new object[] { long.Parse(str) });
                    }
                    var intConstructor = list.FirstOrDefault(c => c.GetParameters().First().ParameterType == typeof(int));
                    if (intConstructor != null)
                    {

                        return str => intConstructor.Invoke(new object[] { int.Parse(str) });
                    }
                    throw new Exception($"类型 {type}必须有一个仅包含long/int/guid类型作为参数的构造函数");
                });
                return func.Invoke(strValue);
            }
            throw new Exception($"无法从{value.GetType()} 转换为 {context.PropertyDescriptor.PropertyType}");
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
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

            throw new ArgumentException($"Cannot convert {value ?? "(null)"} to {destinationType}", nameof(destinationType));
        }
    }
}
