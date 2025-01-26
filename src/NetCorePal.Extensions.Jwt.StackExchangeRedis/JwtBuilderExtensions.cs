using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Jwt;
using NetCorePal.Extensions.Jwt.StackExchangeRedis;

namespace Microsoft.Extensions.DependencyInjection;

public static class JwtBuilderExtensions
{
    public static IJwtBuilder AddRedisStore(this IJwtBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtSettingStore, RedisJwtSettingStore>());
        return builder;
    }
}