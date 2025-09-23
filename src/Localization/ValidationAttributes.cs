using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace NetCorePal.Extensions.Localization.Validation;

/// <summary>
/// Localized required validation attribute
/// </summary>
public class LocalizedRequiredAttribute : RequiredAttribute
{
    public LocalizedRequiredAttribute()
    {
        ErrorMessage = "ValidationRequired";
    }

    public override string FormatErrorMessage(string name)
    {
        try
        {
            return FrameworkLocalizer.GetString(ErrorMessage ?? "ValidationRequired");
        }
        catch
        {
            return base.FormatErrorMessage(name);
        }
    }
}

/// <summary>
/// Localized string length validation attribute
/// </summary>
public class LocalizedStringLengthAttribute : StringLengthAttribute
{
    public LocalizedStringLengthAttribute(int maximumLength) : base(maximumLength)
    {
        ErrorMessage = "ValidationStringLength";
    }

    public LocalizedStringLengthAttribute(int maximumLength, int minimumLength) : base(maximumLength)
    {
        MinimumLength = minimumLength;
        ErrorMessage = "ValidationStringLength";
    }

    public override string FormatErrorMessage(string name)
    {
        try
        {
            return FrameworkLocalizer.GetString(ErrorMessage ?? "ValidationStringLength", MinimumLength, MaximumLength);
        }
        catch
        {
            return base.FormatErrorMessage(name);
        }
    }
}

/// <summary>
/// Localized range validation attribute
/// </summary>
public class LocalizedRangeAttribute : RangeAttribute
{
    public LocalizedRangeAttribute(int minimum, int maximum) : base(minimum, maximum)
    {
        ErrorMessage = "ValidationOutOfRange";
    }

    public LocalizedRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
    {
        ErrorMessage = "ValidationOutOfRange";
    }

    public LocalizedRangeAttribute(Type type, string minimum, string maximum) : base(type, minimum, maximum)
    {
        ErrorMessage = "ValidationOutOfRange";
    }

    public override string FormatErrorMessage(string name)
    {
        try
        {
            return FrameworkLocalizer.GetString(ErrorMessage ?? "ValidationOutOfRange");
        }
        catch
        {
            return base.FormatErrorMessage(name);
        }
    }
}