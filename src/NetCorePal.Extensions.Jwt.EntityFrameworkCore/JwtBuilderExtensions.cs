using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Jwt;
using NetCorePal.Extensions.Jwt.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class JwtBuilderExtensions
{
    public static IJwtBuilder AddEntityFrameworkCoreStore<TDbContext>(this IJwtBuilder builder)
        where TDbContext : DbContext, IJwtSettingDbContext
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IJwtSettingStore, DbContextJwtSettingStore<TDbContext>>());
        return builder;
    }
}