using System.ComponentModel;
using System.Globalization;

namespace NetCorePal.Extensions.Domain;

[TypeConverter(typeof(RowVersionTypeConverter))]
public record RowVersion(int VersionNumber = 0);

public class RowVersionTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(int);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(int);


    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is int intValue)
        {
            return new RowVersion(intValue);
        }

        throw new ArgumentException($"Cannot convert {value ?? "(null)"} to {typeof(RowVersion)}",
            nameof(value));
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
        Type destinationType)
    {
        if (value is RowVersion rowVersion)
        {
            return rowVersion.VersionNumber;
        }

        throw new ArgumentException($"Cannot convert {value ?? "(null)"} to {destinationType}",
            nameof(destinationType));
    }
}