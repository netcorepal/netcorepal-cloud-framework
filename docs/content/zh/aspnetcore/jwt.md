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



在`Startup.cs`中添加如下代码：

```csharp

builder.Services.AddJwtAuthentication(options =>
{
    // 身份认证配置逻辑
});

builder.Services.AddNetCorePalJwt().AddInMemoryStore(); // 使用内存存储密钥
```

如果需要使用文件存储密钥，可以使用以下代码：

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // 身份认证配置逻辑
});

builder.Services.AddNetCorePalJwt().AddFileStore("jwtsetting-filename.json"); // 使用文件存储密钥
```

使用Redis存储密钥：

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // 身份认证配置逻辑
});

// 添加Redis连接
builder.Services.AddSingleton<IConnectionMultiplexer>(p => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddNetCorePalJwt().AddRedisStore(); // 使用Redis存储密钥
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
builder.Services.AddJwtAuthentication(options =>
{
    // 身份认证配置逻辑
});

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddNetCorePalJwt().AddEntityFrameworkCoreStore<MyDbContext>(); // 使用EntityFrameworkCore存储密钥
```


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