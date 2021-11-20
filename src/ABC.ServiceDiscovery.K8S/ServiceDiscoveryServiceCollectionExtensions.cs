using ABC.ServiceDiscovery.Abstractions;
using ABC.ServiceDiscovery.K8S;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceDiscoveryServiceCollectionExtensions
    {
        /// <summary>
        /// 添加服务发现
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configFunc"></param>
        /// <returns></returns>
        public static IServiceCollection AddK8SServiceDiscovery(this IServiceCollection services, Action<K8SProviderOption> configFunc)
        {
            configFunc(new K8SProviderOption());
            services.Configure(configFunc);
            services.AddSingleton<K8SServiceDiscoveryProvider>();
            services.AddSingleton<IServiceDiscoveryProvider>(p => p.GetRequiredService<K8SServiceDiscoveryProvider>());
            return services;
        }
    }
}