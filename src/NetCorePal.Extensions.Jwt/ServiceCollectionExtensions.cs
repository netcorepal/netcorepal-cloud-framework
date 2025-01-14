using NetCorePal.Extensions.Jwt;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IJwtBuilder AddJwt(this IServiceCollection services)
    {
        var builder = new JwtBuilder(services);
        services.AddHostedService<JwtHostedService>();
        return builder;
    }
}