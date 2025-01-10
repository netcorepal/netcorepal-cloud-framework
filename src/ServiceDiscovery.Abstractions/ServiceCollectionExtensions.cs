using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetCorePalServiceDiscoveryClient(this IServiceCollection services)
        {
            services.AddSingleton<IServiceDiscoveryClient, DefaultServiceDiscoveryClient>();
            services.AddSingleton<IServiceSelector, DefaultServiceSelector>();
            return services;
        }
    }
}