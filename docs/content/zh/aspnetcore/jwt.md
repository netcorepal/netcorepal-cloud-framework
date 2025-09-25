# Jwt身份认证

## 介绍

为了方便用户使用，我们提供了管理Jwt密钥的功能，可以自动生成密钥，并注入到`JwtBearerOptions`中。我们提供了
`InMemoryJwtSettingStore`、`FileJwtSettingStore`、`RedisJwtSettingStore`、`DbContextJwtSettingStore`等密钥存储方式，用户可以根据自己的需求选择合适的方式。

## 如何使用

添加包引用：

```shell
# InMemory存储 、File存储
dotnet add package NetCorePal.Extensions.Jwt   

# Redis存储
dotnet add package NetCorePal.Extensions.Jwt.StackExchangeRedis

# EntityFrameworkCore存储
dotnet add package NetCorePal.Extensions.Jwt.EntityFrameworkCore
```

在启动代码中添加如下配置（示例为最简JWT认证配置）：

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

// 配置 JWT 认证（实际校验参数会由后台服务动态更新）
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

// 注册 NetCorePal Jwt，并选择密钥存储方式（示例：内存）
builder.Services.AddNetCorePalJwt()
    .AddInMemoryStore();
```

如果需要使用文件存储密钥，可以使用以下代码：

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

builder.Services.AddNetCorePalJwt()
    .AddFileStore("jwtsetting-filename.json"); // 使用文件存储密钥
```

使用Redis存储密钥：

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StackExchange.Redis;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

// 添加Redis连接
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddNetCorePalJwt()
    .AddRedisStore(); // 使用Redis存储密钥
```

使用EntityFrameworkCore存储密钥，需要在MyDbContext中添加`JwtSetting`实体类：

```csharp
public class MyDbContext : DbContext , IJwtSettingDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<JwtSetting> JwtSettings => Set<JwtSetting>();
}
```

配置身份认证及存储：

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddNetCorePalJwt()
    .AddEntityFrameworkCoreStore<MyDbContext>(); // 使用EntityFrameworkCore存储密钥
```

## 密钥轮转（Key Rotation）

从现在起，密钥轮转服务 `JwtKeyRotationService` 会在调用 `AddNetCorePalJwt()` 时自动注册，并由后台服务（`JwtHostedService`）按配置定期执行。

如需启用并自定义轮转策略，请通过 `AddNetCorePalJwt` 的配置参数设置 `JwtOptions`：

```csharp
builder.Services.AddNetCorePalJwt(options =>
{
    options.AutomaticRotationEnabled = true;                    // 启用自动轮转
    options.KeyLifetime = TimeSpan.FromDays(30);                // 密钥有效期（仅在 AutomaticRotationEnabled = true 时生效）
    options.RotationCheckInterval = TimeSpan.FromHours(1);      // 检查轮转的间隔
    options.ExpiredKeyRetentionPeriod = TimeSpan.FromDays(30);  // 过期密钥保留时长（用于验证旧 token）
    options.MaxActiveKeys = 2;                                  // 同时保留的活跃密钥数量
})
.AddInMemoryStore();
```

注意：当 `AutomaticRotationEnabled` 为 false 时，系统会为新生成的密钥设置一个很长的有效期（100 年），此时 `KeyLifetime` 配置将被忽略。

提示：单实例运行时，`AddNetCorePalJwt()` 会默认使用内存锁进行同步；如果是多实例/分布式部署，建议配置分布式锁（例如 Redis）以避免并发轮转冲突：

```csharp
// 需要引用分布式锁包：NetCorePal.Extensions.DistributedLocks.Redis
// 并注册 Redis 连接 IConnectionMultiplexer
builder.Services.AddRedisLocks(); // 或 AddRedisLocks(connectionMultiplexer)
```

## 数据保护

使用ASP.NET Core DataProtection保护存储的JWT密钥：

```csharp
builder.Services.AddNetCorePalJwt()
    .UseDataProtection() // 启用密钥加密存储（请在选择存储之前调用）
    .AddFileStore("jwtsetting-filename.json");
```

重要说明：必须在选择存储之前调用 `UseDataProtection`（如 `AddInMemoryStore`、`AddFileStore`、`AddRedisStore`、`AddEntityFrameworkCoreStore` 之前）。该方法会对“接下来要注册的” `IJwtSettingStore` 进行包装加密；如果在已注册具体存储之后再调用，则不会对已注册的存储生效。

DataProtection会自动加密存储的私钥数据，确保密钥在文件、数据库或Redis中的安全性。

## 生成JwtToken

在控制器中，可以使用`IJwtProvider`接口生成JwtToken，如下所示：

```csharp

[HttpPost("/jwtlogin")]
public async Task<ResponseData<string>> JwtLogin(string name, [FromServices] IJwtProvider provider)
{
    var claims = new[]
    {
        new Claim("uid", "111"),
        new Claim("type", "client"),
        new Claim("email", "abc@efg.com"),
    };
    var jwt = await provider.GenerateJwtToken(new JwtData("issuer-x", "audience-y",
        claims,
        DateTime.Now,
        DateTime.Now.AddMinutes(1)));
    return jwt.AsResponseData();
}
```