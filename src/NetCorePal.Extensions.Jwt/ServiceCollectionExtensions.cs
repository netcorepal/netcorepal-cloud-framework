using NetCorePal.Extensions.Jwt;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IJwtBuilder AddNetCorePalJwt(this IServiceCollection services)
    {
        var builder = new JwtBuilder(services);
        services.AddHostedService<JwtHostedService>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        
        // Configure default rotation options
        services.Configure<JwtKeyRotationOptions>(_ => { });
        
        // Add key rotation services
        services.AddSingleton<IJwtKeyRotationService, JwtKeyRotationService>();
        services.AddHostedService<JwtKeyRotationBackgroundService>();
        
        return builder;
    }
    
    public static IJwtBuilder AddNetCorePalJwt(this IServiceCollection services, Action<JwtKeyRotationOptions> configureRotation)
    {
        services.Configure(configureRotation);
        return services.AddNetCorePalJwt();
    }
}