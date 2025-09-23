# 国际化 (i18n) 支持

## 概述

NetCorePal Cloud Framework 提供了全面的国际化支持，用于构建多语言应用程序。框架包含了常见系统消息的内置中英文资源，以及为应用程序开发者提供添加自己本地化内容的灵活架构。

## 功能特性

- **框架本地化**：常见框架消息的内置中英文翻译
- **应用程序本地化**：开发者本地化应用程序的易用API
- **自动文化检测**：支持从请求头、查询字符串、Cookie和路由检测用户文化
- **本地化验证**：带有本地化错误消息的数据注解属性
- **本地化异常**：支持本地化错误消息的异常类
- **ASP.NET Core 集成**：与ASP.NET Core本地化基础设施的无缝集成

## 快速开始

### 1. 添加包引用

```xml
<PackageReference Include="NetCorePal.Extensions.Localization" />
```

### 2. 配置服务

在你的 `Program.cs` 中，添加本地化服务：

```csharp
builder.Services.AddNetCorePalLocalizationWithAspNetCore(options =>
{
    options.DefaultCulture = "en";
    options.SupportedCultures = new[] { "en", "zh" };
    options.FallbackToDefaultCulture = true;
});
```

### 3. 配置中间件

在管道中添加本地化中间件：

```csharp
var app = builder.Build();

// 必须在管道早期调用
app.UseNetCorePalLocalization();

app.UseRouting();
// ... 其他中间件
```

## 使用示例

### 使用框架资源

框架提供预翻译的常见消息：

```csharp
public class MyService
{
    public void DoSomething()
    {
        // 获取本地化的框架消息
        var successMsg = FrameworkLocalizer.GetString("OperationSuccessful");
        var errorMsg = FrameworkLocalizer.GetString("InvalidEntityId", entityId);
        
        // 支持中英文
        // 英文: "Operation completed successfully"
        // 中文: "操作成功完成"
    }
}
```

### 应用程序特定的本地化

对于你自己的应用程序资源：

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

### 本地化验证

使用本地化验证属性：

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

### 本地化异常

抛出带有本地化消息的异常：

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

## 文化检测

框架自动从多个来源检测用户文化：

1. **查询字符串**：`?culture=zh`
2. **路由值**：`/zh/api/users`
3. **Cookie**：通过 `CookieRequestCultureProvider` 设置
4. **Accept-Language 头**：浏览器语言偏好

API 调用示例：

```bash
# 带查询字符串文化的请求
curl "https://api.example.com/api/localization/messages?culture=zh"

# 带Accept-Language头的请求
curl -H "Accept-Language: zh-CN" "https://api.example.com/api/localization/messages"
```

## 可用的框架资源

框架提供以下预翻译消息：

| 键 | 英文 | 中文 |
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

## 添加自定义资源

要添加你自己的本地化资源：

1. **创建资源文件** 遵循 .NET 约定：
   ```
   Resources/
     MyResources.resx         // 默认 (英文)
     MyResources.zh.resx      // 中文
     MyResources.ja.resx      // 日文
   ```

2. **注册和使用资源**：
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

## 配置选项

```csharp
services.AddNetCorePalLocalizationWithAspNetCore(options =>
{
    // 未指定时的默认文化
    options.DefaultCulture = "en";
    
    // 应用程序支持的文化
    options.SupportedCultures = new[] { "en", "zh", "ja", "fr" };
    
    // 当翻译缺失时是否回退到默认文化
    options.FallbackToDefaultCulture = true;
});
```

## 最佳实践

1. **始终使用资源键**：永远不要硬编码面向用户的字符串
2. **提供完整翻译**：确保所有支持的文化都有完整的翻译
3. **使用描述性键**：让资源键自我记录
4. **测试所有文化**：验证应用程序在所有支持的文化中都能正常工作
5. **考虑文本扩展**：某些语言需要比其他语言更多的空间
6. **处理复数化**：不同语言有不同的复数规则

## 集成示例

### 与领域驱动设计

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

### 与多租户系统

本地化与框架的多租户支持无缝配合：

```csharp
public class TenantAwareController : ControllerBase
{
    public IActionResult GetTenantSpecificMessage()
    {
        // 文化自动检测
        // 租户上下文得到保留
        var message = FrameworkLocalizer.GetString("WelcomeMessage");
        return Ok(message);
    }
}
```

### 与异常处理中间件

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
            // 异常消息已经本地化
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(ex.Message);
        }
    }
}
```

## 故障排除

### 常见问题

1. **本地化不工作**
   - 确保在管道中调用了 `UseNetCorePalLocalization()`
   - 验证文化设置正确
   - 检查资源文件命名约定

2. **缺少翻译**
   - 验证资源文件是嵌入资源
   - 检查资源键拼写
   - 确保回退文化有所有键

3. **文化未检测到**
   - 检查请求头/查询参数
   - 验证支持的文化配置
   - 测试文化提供者顺序

### 调试

启用详细日志记录来排除问题：

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

## 迁移指南

如果你正在从现有本地化迁移：

1. **安装包**：添加 `NetCorePal.Extensions.Localization`
2. **更新配置**：替换现有本地化设置
3. **更新资源访问**：对框架资源使用 `FrameworkLocalizer`
4. **更新验证**：用本地化版本替换验证属性
5. **更新异常**：使用本地化异常类

框架设计为与标准 .NET 本地化模式向后兼容。

## 演示API

框架在测试项目中提供了一个演示控制器，展示了所有本地化功能：

```bash
# 获取英文消息
curl "https://localhost:5001/api/localizationdemo/messages"

# 获取中文消息
curl "https://localhost:5001/api/localizationdemo/messages?culture=zh"

# 测试本地化验证
curl -X POST "https://localhost:5001/api/localizationdemo/register" \
     -H "Content-Type: application/json" \
     -d '{"name":"","email":"","age":0}'

# 测试本地化异常
curl "https://localhost:5001/api/localizationdemo/user/999?culture=zh"
```

这个实现提供了企业级的国际化解决方案，支持框架本身和应用程序开发者的多语言需求。