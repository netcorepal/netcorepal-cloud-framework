# NetCorePal Localization Extensions

This package provides comprehensive internationalization (i18n) support for NetCorePal cloud framework, including both framework-level and application-level localization capabilities.

## Features

- **Framework Text Resources**: Built-in Chinese and English localization for common framework messages
- **Application Localization Support**: Easy-to-use APIs for application developers to add their own localized resources
- **ASP.NET Core Integration**: Automatic culture detection from headers, query strings, and routes
- **Localized Validation Attributes**: Data annotation attributes with localized error messages
- **Localized Exceptions**: Exception classes that support localized error messages
- **Flexible Configuration**: Configurable default culture and supported cultures

## Quick Start

### 1. Add Package Reference

```xml
<PackageReference Include="NetCorePal.Extensions.Localization" />
```

### 2. Configure Services

```csharp
// In Program.cs
builder.Services.AddNetCorePalLocalizationWithAspNetCore(options =>
{
    options.DefaultCulture = "en";
    options.SupportedCultures = new[] { "en", "zh" };
    options.FallbackToDefaultCulture = true;
});
```

### 3. Configure Middleware

```csharp
// In Program.cs
var app = builder.Build();

app.UseNetCorePalLocalization(); // Must be called before other middleware that needs localization

app.UseRouting();
// ... other middleware
```

### 4. Use Localization in Your Code

#### Using Framework Resources

```csharp
public class MyService
{
    public void DoSomething()
    {
        // Access framework localized strings
        var message = FrameworkLocalizer.GetString("OperationSuccessful");
        var errorMsg = FrameworkLocalizer.GetString("InvalidEntityId", entityId);
    }
}
```

#### Using Application-Specific Resources

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

#### Using Localized Validation

```csharp
public class CreateUserRequest
{
    [LocalizedRequired]
    [LocalizedStringLength(100, 3)]
    public string Name { get; set; } = string.Empty;

    [LocalizedRequired]
    public string Email { get; set; } = string.Empty;
}
```

#### Using Localized Exceptions

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

    public void ValidateUser(User user)
    {
        if (user.Age < 18)
        {
            throw new BusinessRuleViolationException("UserMustBeAdult", user.Age);
        }
    }
}
```

## Culture Detection

The framework automatically detects the user's preferred culture from:

1. Query string: `?culture=zh`
2. Cookie: `CookieRequestCultureProvider`
3. Accept-Language header: Browser language settings

## Adding Your Own Resources

Create resource files following .NET conventions:

```
Resources/
  MyResources.resx      // Default (English)
  MyResources.zh.resx   // Chinese
```

Register and use them:

```csharp
// In your service
public class MyService
{
    private readonly IStringLocalizer<MyResources> _localizer;

    public MyService(IStringLocalizer<MyResources> localizer)
    {
        _localizer = localizer;
    }

    public string GetMessage()
    {
        return _localizer["MyMessage"];
    }
}
```

## Built-in Framework Resources

The framework provides localized versions of common messages:

- `ValidationRequired` - "This field is required" / "此字段是必填项"
- `ValidationInvalidFormat` - "The format is invalid" / "格式无效"
- `UnknownError` - "An unknown error occurred" / "发生未知错误"
- `OperationSuccessful` - "Operation completed successfully" / "操作成功完成"
- `NotFound` - "Resource not found" / "未找到资源"
- `Unauthorized` - "Unauthorized access" / "未授权访问"
- And many more...

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

1. **Always use resource keys**: Never hardcode user-facing strings
2. **Provide fallbacks**: Ensure your default culture has all strings
3. **Use descriptive keys**: Make resource keys self-documenting
4. **Test all cultures**: Verify your application works in all supported cultures
5. **Consider pluralization**: Different languages have different plural rules

## Integration with Existing Projects

This localization system integrates seamlessly with existing NetCorePal framework features:

- Context transmission system
- Domain-driven design patterns
- Multi-tenant scenarios
- Exception handling middleware