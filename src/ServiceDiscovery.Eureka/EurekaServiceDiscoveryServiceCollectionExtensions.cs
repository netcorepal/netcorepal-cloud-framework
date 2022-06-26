using NetCorePal.ServiceDiscovery;
using NetCorePal.ServiceDiscovery.Eureka;
using Steeltoe.Discovery.Eureka;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EurekaServiceDiscoveryServiceCollectionExtensions
    {
        public static IServiceCollection AddEurekaServiceDiscovery(this IServiceCollection services, Action<EurekaProviderOption> configAction)
        {
            configAction(new EurekaProviderOption());
            services.Configure(configAction);
            services.AddSingleton<IServiceDiscoveryProvider, EurekaServiceDiscoveryProvider>();
            return services;
        }
    }
}
