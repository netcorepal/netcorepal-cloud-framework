# Internationalization (i18n) Support

## Overview

NetCorePal Cloud Framework provides comprehensive internationalization support for building multi-language applications. The framework includes built-in Chinese and English resources for common system messages, as well as a flexible architecture for application developers to add their own localized content.

## Features

- **Framework Localization**: Built-in Chinese and English translations for common framework messages
- **Application Localization**: Easy-to-use APIs for developers to localize their applications
- **Automatic Culture Detection**: Support for detecting user culture from headers, query strings, cookies, and routes
- **Localized Validation**: Data annotation attributes with localized error messages
- **Localized Exceptions**: Exception classes that support localized error messages
- **ASP.NET Core Integration**: Seamless integration with ASP.NET Core localization infrastructure

## Quick Start

### 1. Add Package Reference

```xml
<PackageReference Include="NetCorePal.Extensions.Localization" />
```

### 2. Configure Services

In your `Program.cs`, add localization services:

```csharp
builder.Services.AddNetCorePalLocalizationWithAspNetCore(options =>
{
    options.DefaultCulture = "en";
    options.SupportedCultures = new[] { "en", "zh" };
    options.FallbackToDefaultCulture = true;
});
```

### 3. Configure Middleware

Add the localization middleware in your pipeline:

```csharp
var app = builder.Build();

// Must be called early in the pipeline
app.UseNetCorePalLocalization();

app.UseRouting();
// ... other middleware
```

## Usage Examples

### Using Framework Resources

The framework provides pre-translated common messages:

```csharp
public class MyService
{
    public void DoSomething()
    {
        // Get localized framework messages
        var successMsg = FrameworkLocalizer.GetString("OperationSuccessful");
        var errorMsg = FrameworkLocalizer.GetString("InvalidEntityId", entityId);
        
        // Available in both English and Chinese
        // English: "Operation completed successfully"
        // Chinese: "操作成功完成"
    }
}
```

### Application-Specific Localization

For your own application resources:

```csharp
public class MyController : ControllerBase
{
    private readonly IStringLocalizer<MyController> _localizer;

    public MyController(IStringLocalizer<MyController> localizer)
    {
        _localizer = localizer;
    }

    public IActionResult Get()
    {
        var message = _localizer["WelcomeMessage"];
        return Ok(message);
    }
}
```

### Localized Validation

Use localized validation attributes:

```csharp
public class CreateUserRequest
{
    [LocalizedRequired]
    [LocalizedStringLength(50, 2)]
    public string Name { get; set; } = string.Empty;

    [LocalizedRequired]
    public string Email { get; set; } = string.Empty;

    [LocalizedRange(18, 120)]
    public int Age { get; set; }
}
```

### Localized Exceptions

Throw exceptions with localized messages:

```csharp
public class UserService
{
    public User GetUser(int id)
    {
        var user = _repository.Find(id);
        if (user == null)
        {
            throw new EntityNotFoundException(id);
        }
        return user;
    }
}
```

## Culture Detection

The framework automatically detects user culture from multiple sources:

1. **Query String**: `?culture=zh`
2. **Route Values**: `/zh/api/users`
3. **Cookies**: Set via `CookieRequestCultureProvider`
4. **Accept-Language Header**: Browser language preferences

Example API calls:

```bash
# Request with query string culture
curl "https://api.example.com/api/localization/messages?culture=zh"

# Request with Accept-Language header
curl -H "Accept-Language: zh-CN" "https://api.example.com/api/localization/messages"
```

## Available Framework Resources

The framework provides the following pre-translated messages:

| Key | English | Chinese |
|-----|---------|---------|
| `ValidationRequired` | "This field is required." | "此字段是必填项。" |
| `ValidationInvalidFormat` | "The format is invalid." | "格式无效。" |
| `ValidationOutOfRange` | "The value is out of range." | "值超出范围。" |
| `ValidationStringLength` | "The string length must be between {0} and {1} characters." | "字符串长度必须在 {0} 和 {1} 个字符之间。" |
| `UnknownError` | "An unknown error occurred." | "发生未知错误。" |
| `OperationSuccessful` | "Operation completed successfully." | "操作成功完成。" |
| `OperationFailed` | "Operation failed." | "操作失败。" |
| `NotFound` | "Resource not found." | "未找到资源。" |
| `Unauthorized` | "Unauthorized access." | "未授权访问。" |
| `Forbidden` | "Access forbidden." | "访问被禁止。" |
| `InvalidEntityId` | "Invalid entity ID: {0}" | "无效的实体ID: {0}" |
| `DomainEventProcessingFailed` | "Domain event processing failed: {0}" | "领域事件处理失败: {0}" |
| `AggregateNotFound` | "Aggregate with ID {0} not found." | "未找到ID为 {0} 的聚合根。" |
| `TransactionRolledBack` | "Transaction was rolled back." | "事务已回滚。" |
| `ConcurrencyConflict` | "Concurrency conflict detected. Please retry the operation." | "检测到并发冲突。请重试操作。" |

## Adding Custom Resources

To add your own localized resources:

1. **Create Resource Files** following .NET conventions:
   ```
   Resources/
     MyResources.resx         // Default (English)
     MyResources.zh.resx      // Chinese
     MyResources.ja.resx      // Japanese
   ```

2. **Register and Use Resources**:
   ```csharp
   public class MyService
   {
       private readonly IStringLocalizer<MyResources> _localizer;

       public MyService(IStringLocalizer<MyResources> localizer)
       {
           _localizer = localizer;
       }

       public string GetLocalizedMessage()
       {
           return _localizer["MyCustomMessage"];
       }
   }
   ```

## Configuration Options

```csharp
services.AddNetCorePalLocalizationWithAspNetCore(options =>
{
    // Default culture when none is specified
    options.DefaultCulture = "en";
    
    // Cultures supported by your application  
    options.SupportedCultures = new[] { "en", "zh", "ja", "fr" };
    
    // Whether to fall back to default culture when translation is missing
    options.FallbackToDefaultCulture = true;
});
```

## Best Practices

1. **Always Use Resource Keys**: Never hardcode user-facing strings
2. **Provide Complete Translations**: Ensure all supported cultures have complete translations
3. **Use Descriptive Keys**: Make resource keys self-documenting
4. **Test All Cultures**: Verify your application works correctly in all supported cultures
5. **Consider Text Expansion**: Some languages require more space than others
6. **Handle Pluralization**: Different languages have different plural rules

## Integration Examples

### With Domain-Driven Design

```csharp
public class OrderDomainService
{
    public void ValidateOrder(Order order)
    {
        if (order.Items.Count == 0)
        {
            throw new DomainException("OrderMustHaveItems");
        }
        
        if (order.Total <= 0)
        {
            throw new DomainException("OrderTotalMustBePositive", order.Total);
        }
    }
}
```

### With Multi-Tenant Systems

Localization works seamlessly with the framework's multi-tenant support:

```csharp
public class TenantAwareController : ControllerBase
{
    public IActionResult GetTenantSpecificMessage()
    {
        // Culture is automatically detected
        // Tenant context is preserved
        var message = FrameworkLocalizer.GetString("WelcomeMessage");
        return Ok(message);
    }
}
```

### With Exception Handling Middleware

```csharp
public class LocalizedExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (LocalizedException ex)
        {
            // Exception message is already localized
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(ex.Message);
        }
    }
}
```

## Troubleshooting

### Common Issues

1. **Localization Not Working**
   - Ensure `UseNetCorePalLocalization()` is called in the pipeline
   - Verify culture is being set correctly
   - Check resource file naming conventions

2. **Missing Translations**
   - Verify resource files are embedded resources
   - Check resource key spelling
   - Ensure fallback culture has all keys

3. **Culture Not Detected**
   - Check request headers/query parameters
   - Verify supported cultures configuration
   - Test culture providers order

### Debugging

Enable detailed logging to troubleshoot issues:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

## Migration Guide

If you're migrating from existing localization:

1. **Install Package**: Add `NetCorePal.Extensions.Localization`
2. **Update Configuration**: Replace existing localization setup
3. **Update Resource Access**: Use `FrameworkLocalizer` for framework resources
4. **Update Validation**: Replace validation attributes with localized versions
5. **Update Exceptions**: Use localized exception classes

The framework is designed to be backward compatible with standard .NET localization patterns.