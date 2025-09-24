using NetCorePal.Extensions.Jwt;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IJwtBuilder AddNetCorePalJwt(this IServiceCollection services)
    {
        var builder = new JwtBuilder(services);
        services.AddSingleton<JwtHostedService>();
        services.AddHostedService<JwtHostedService>(provider => provider.GetRequiredService<JwtHostedService>());
        services.AddSingleton<IJwtProvider, JwtProvider>();
        
        return builder;
    }
    
    public static IJwtBuilder AddNetCorePalJwt(this IServiceCollection services, Action<JwtKeyRotationOptions> configureRotation)
    {
        services.Configure(configureRotation);
        return services.AddNetCorePalJwt();
    }
}