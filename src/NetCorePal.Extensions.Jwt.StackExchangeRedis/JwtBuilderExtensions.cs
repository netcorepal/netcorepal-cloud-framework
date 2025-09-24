using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Jwt;
using NetCorePal.Extensions.Jwt.StackExchangeRedis;

namespace Microsoft.Extensions.DependencyInjection;

public static class JwtBuilderExtensions
{
    public static IJwtBuilder AddRedisStore(this IJwtBuilder builder)
    {
        builder.Services.AddSingleton<RedisJwtSettingStore>();
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtSettingStore>(provider => provider.GetRequiredService<RedisJwtSettingStore>()));
        return builder;
    }
}