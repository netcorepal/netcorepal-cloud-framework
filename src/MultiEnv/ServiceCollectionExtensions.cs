using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.ServiceDiscovery;
using NetCorePal.Extensions.MultiEnv;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnvServiceSelector(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IServiceSelector, EnvServiceSelector>());
            return services;
        }
    }
}