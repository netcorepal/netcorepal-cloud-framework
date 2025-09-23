namespace NetCorePal.Extensions.Localization.Exceptions;

/// <summary>
/// Exception that supports localized messages
/// </summary>
public class LocalizedException : Exception
{
    /// <summary>
    /// Resource key for the localized message
    /// </summary>
    public string ResourceKey { get; }

    /// <summary>
    /// Arguments for string formatting
    /// </summary>
    public object[] Arguments { get; }

    /// <summary>
    /// Creates a new localized exception
    /// </summary>
    /// <param name="resourceKey">Resource key for localization</param>
    /// <param name="arguments">Arguments for string formatting</param>
    public LocalizedException(string resourceKey, params object[] arguments) 
        : base(GetLocalizedMessage(resourceKey, arguments))
    {
        ResourceKey = resourceKey;
        Arguments = arguments;
    }

    /// <summary>
    /// Creates a new localized exception with inner exception
    /// </summary>
    /// <param name="resourceKey">Resource key for localization</param>
    /// <param name="innerException">Inner exception</param>
    /// <param name="arguments">Arguments for string formatting</param>
    public LocalizedException(string resourceKey, Exception innerException, params object[] arguments)
        : base(GetLocalizedMessage(resourceKey, arguments), innerException)
    {
        ResourceKey = resourceKey;
        Arguments = arguments;
    }

    private static string GetLocalizedMessage(string resourceKey, object[] arguments)
    {
        try
        {
            return FrameworkLocalizer.GetString(resourceKey, arguments);
        }
        catch
        {
            // Fallback to resource key if localization fails
            return resourceKey;
        }
    }
}

/// <summary>
/// Domain-specific localized exceptions
/// </summary>
public class DomainException : LocalizedException
{
    public DomainException(string resourceKey, params object[] arguments) 
        : base(resourceKey, arguments)
    {
    }

    public DomainException(string resourceKey, Exception innerException, params object[] arguments)
        : base(resourceKey, innerException, arguments)
    {
    }
}

/// <summary>
/// Business logic violation exception
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string resourceKey, params object[] arguments)
        : base(resourceKey, arguments)
    {
    }
}

/// <summary>
/// Entity not found exception
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(object entityId)
        : base("AggregateNotFound", entityId)
    {
    }

    public EntityNotFoundException(string resourceKey, params object[] arguments)
        : base(resourceKey, arguments)
    {
    }
}