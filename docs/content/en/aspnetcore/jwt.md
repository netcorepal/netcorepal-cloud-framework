# Jwt Authentication

## Introduction

To facilitate user usage, we provide the functionality to manage Jwt keys, which can automatically generate keys and inject them into `JwtBearerOptions`. We offer various key storage methods such as `InMemoryJwtSettingStore`, `FileJwtSettingStore`, `RedisJwtSettingStore`, and `DbContextJwtSettingStore`, allowing users to choose the appropriate method based on their needs.

## How to Use

Add package references:

```shell
# InMemory storage, File storage
dotnet add package NetCorePal.Extensions.Jwt   

# Redis storage
dotnet add package NetCorePal.Extensions.Jwt.StackExchangeRedis

# EntityFrameworkCore storage
dotnet add package NetCorePal.Extensions.Jwt.EntityFrameworkCore

```

Add the following code in `Startup.cs`:

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // Authentication configuration logic
});

builder.Services.AddNetCorePalJwt().AddInMemoryStore(); // Use in-memory storage for keys
```

If you need to use file storage for keys, you can use the following code:

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // Authentication configuration logic
});

builder.Services.AddNetCorePalJwt().AddFileStore("jwtsetting-filename.json"); // Use file storage for keys
```

To use Redis for key storage:

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // Authentication configuration logic
});

// Add Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(p => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddNetCorePalJwt().AddRedisStore(); // Use Redis for key storage
```

To use EntityFrameworkCore for key storage, you need to add the `JwtSetting` entity class in `MyDbContext`:

```csharp
public class MyDbContext : DbContext, IJwtSettingDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<JwtSetting> JwtSettings => Set<JwtSetting>();
}
```

Configure authentication and storage:

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // Authentication configuration logic
});

builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddNetCorePalJwt().AddEntityFrameworkCoreStore<MyDbContext>(); // Use EntityFrameworkCore for key storage
```

## Generate JwtToken

In the controller, you can use the `IJwtProvider` interface to generate JwtToken as shown below:

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