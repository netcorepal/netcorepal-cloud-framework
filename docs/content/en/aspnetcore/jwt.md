# JWT Authentication

## Introduction

To facilitate user usage, we provide the function of managing JWT keys, which can automatically generate keys and inject them into `JwtBearerOptions`. We provide two key storage methods: `InMemoryJwtSettingStore`, `FileJwtSettingStore`, and `RedisJwtSettingStore`. Users can choose the appropriate method according to their needs.

## How to Use

First, add the following code in `Startup.cs`:

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // Authentication configuration logic
});

builder.Services.AddNetCorePalJwt().AddInMemoryStore(); // Use in-memory key storage
```

If you need to use file key storage, you can use the following code:

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // Authentication configuration logic
});

builder.Services.AddNetCorePalJwt().AddFileStore("jwtsetting-filename.json"); // Use file key storage
```

Or use Redis key storage:

```csharp
builder.Services.AddJwtAuthentication(options =>
{
    // Authentication configuration logic
});

// Add Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(p => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddNetCorePalJwt().AddRedisStore(); // Use Redis key storage
```

## Generate JWT Token

In the controller, you can use the `IJwtProvider` interface to generate a JWT token, as shown below:

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