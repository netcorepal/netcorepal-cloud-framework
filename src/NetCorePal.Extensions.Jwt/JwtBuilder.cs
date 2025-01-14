using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Jwt;

public interface IJwtBuilder
{
    IServiceCollection Services { get; }
}

class JwtBuilder : IJwtBuilder
{
    public JwtBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}