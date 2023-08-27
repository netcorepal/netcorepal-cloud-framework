using NetCorePal.ServiceDiscovery.K8S;
using NetCorePal.ServiceDiscovery;
using k8s;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class K8SServiceDiscoveryServiceCollectionExtensions
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
            services.AddKubernetesCore();
            services.AddSingleton<K8SServiceDiscoveryProvider>();
            services.AddHostedService(p => p.GetRequiredService<K8SServiceDiscoveryProvider>());
            services.AddSingleton<IServiceDiscoveryProvider>(p => p.GetRequiredService<K8SServiceDiscoveryProvider>());
            return services;
        }


        internal static IServiceCollection AddKubernetesCore(this IServiceCollection services)
        {
            if (!services.Any(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IKubernetes)))
            {
                services = services.AddSingleton<IKubernetes>(sp =>
                {
                    return new k8s.Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
                });
            }
            return services;
        }

    }
}