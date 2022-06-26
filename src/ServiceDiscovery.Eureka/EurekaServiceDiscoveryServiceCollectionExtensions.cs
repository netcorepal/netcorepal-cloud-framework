using NetCorePal.ServiceDiscovery;
using NetCorePal.ServiceDiscovery.Eureka;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class EurekaServiceDiscoveryServiceCollectionExtensions
    {
        public static IServiceCollection AddEurekaServiceDiscovery(this IServiceCollection services)
        {
            services.AddSingleton<IServiceDiscoveryProvider, EurekaServiceDiscoveryProvider>();
            return services;
        }
    }
}
