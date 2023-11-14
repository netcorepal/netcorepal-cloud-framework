using DotNetCore.CAP.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.MultiEnv.CAP.RabbitMQ;

namespace NetCorePal.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEnvFixedConnectionChannelPool(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<IConnectionChannelPool, EnvFixedConnectionChannelPool>());
        return services;
    }
}