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

Add the following configuration in your startup code (a minimal JWT setup):

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

// Configure JWT authentication (validation parameters will be updated dynamically by the background service)
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

// Register NetCorePal Jwt and choose a key store (example: in-memory)
builder.Services.AddNetCorePalJwt()
    .AddInMemoryStore();
```

If you need file storage for keys:

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
    .AddFileStore("jwtsetting-filename.json"); // Use file storage for keys
```

Use Redis for key storage:

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

// Add Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddNetCorePalJwt()
    .AddRedisStore(); // Use Redis for key storage
```

To use EntityFrameworkCore for key storage, add the `JwtSetting` entity to your `MyDbContext`:

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
    .AddEntityFrameworkCoreStore<MyDbContext>(); // Use EntityFrameworkCore for key storage
```

## Key Rotation

Key rotation (`JwtKeyRotationService`) is registered automatically when you call `AddNetCorePalJwt()` and is executed periodically by the background service (`JwtHostedService`).

To enable and customize rotation, configure `JwtOptions` via `AddNetCorePalJwt`:

```csharp
builder.Services.AddNetCorePalJwt(options =>
{
    options.AutomaticRotationEnabled = true;                  // Enable automatic rotation
    options.KeyLifetime = TimeSpan.FromDays(30);              // Key validity (applies only when AutomaticRotationEnabled = true)
    options.RotationCheckInterval = TimeSpan.FromHours(1);    // Rotation check interval
    options.ExpiredKeyRetentionPeriod = TimeSpan.FromDays(30);// Keep expired keys to validate existing tokens
    options.MaxActiveKeys = 2;                                // Maximum number of active keys to keep
})
.AddInMemoryStore();
```

Note: When `AutomaticRotationEnabled` is false, newly generated keys are assigned a very long lifetime (100 years) and `KeyLifetime` is ignored.

Note: For single-instance scenarios, `AddNetCorePalJwt()` defaults to an in-memory lock for synchronization. For multi-instance/distributed deployments, configure a distributed lock (e.g., Redis) to avoid concurrent rotation conflicts:

```csharp
// Requires NetCorePal.Extensions.DistributedLocks.Redis package
// and a registered IConnectionMultiplexer
builder.Services.AddRedisLocks(); // or AddRedisLocks(connectionMultiplexer)
```

## Data Protection

Use ASP.NET Core DataProtection to protect stored JWT keys:

```csharp
builder.Services.AddNetCorePalJwt()
    .UseDataProtection() // Enable encrypted key storage (call BEFORE selecting a store)
    .AddFileStore("jwtsetting-filename.json");
```

Important: Call `UseDataProtection` before selecting a store (e.g., `AddInMemoryStore`, `AddFileStore`, `AddRedisStore`, `AddEntityFrameworkCoreStore`). The DataProtection wrapper decorates the next `IJwtSettingStore` registration; if called after a store is already registered, encryption will not be applied to that store.

DataProtection automatically encrypts stored private key data, ensuring security of keys in files, databases, or Redis.

## Generate JwtToken

In an endpoint, you can use the `IJwtProvider` interface to generate a JwtToken:

```csharp
public class JwtLoginEndpoint : Endpoint<JwtLoginRequest, ResponseData<string>>
{
    public override void Configure()
    {
        Post("/jwtlogin");
        AllowAnonymous();
    }

    public override async Task HandleAsync(JwtLoginRequest req, CancellationToken ct)
    {
        var provider = Resolve<IJwtProvider>();
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
        await SendAsync(jwt.AsResponseData(), cancellation: ct);
    }
}

public record JwtLoginRequest(string Name);
```