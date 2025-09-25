using NetCorePal.Extensions.Jwt;
using NetCorePal.Extensions.DistributedLocks;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IJwtBuilder AddNetCorePalJwt(this IServiceCollection services)
    {
        return services.AddNetCorePalJwt(option => { });
    }

    public static IJwtBuilder AddNetCorePalJwt(this IServiceCollection services, Action<JwtOptions> configure)
    {
        services.Configure(configure);
        var builder = new JwtBuilder(services);
        services.AddSingleton<JwtHostedService>();
        services.AddHostedService<JwtHostedService>(provider => provider.GetRequiredService<JwtHostedService>());
        services.AddSingleton<IJwtProvider, JwtProvider>();
        builder.Services.AddSingleton<IJwtKeyRotationService, JwtKeyRotationService>();
        return builder;
    }
}