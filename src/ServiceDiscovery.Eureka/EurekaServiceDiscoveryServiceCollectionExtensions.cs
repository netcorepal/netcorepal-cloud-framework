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
            services.AddSingleton<DiscoveryClient>(p =>
            {
                var option = p.GetService<EurekaProviderOption>();

                if (option == null) { throw new ArgumentNullException(nameof(option)); }
                var eco = new EurekaClientOptions
                {
                    ShouldOnDemandUpdateStatusChange = true,
                    ServiceUrl = option.ServerUrl,
                    ShouldRegisterWithEureka = option.RegisterService,
                    ShouldFilterOnlyUpInstances = option.OnlyUpInstances,
                    ShouldGZipContent = option.GZipContent,
                    EurekaServerConnectTimeoutSeconds = option.ConnectTimeoutSeconds,
                    ValidateCertificates = option.ValidateCertificates,
                    RegistryFetchIntervalSeconds = option.FetchIntervalSeconds
                };
                return new DiscoveryClient(eco);
            });
            services.AddSingleton<IServiceDiscoveryProvider, EurekaServiceDiscoveryProvider>();
            return services;
        }
    }
}
