using NetCorePal.Extensions.ServiceDiscovery.K8s;
using NetCorePal.Extensions.ServiceDiscovery;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        public static IServiceCollection AddK8SServiceDiscovery(this IServiceCollection services,
            Action<K8SProviderOption> configFunc)
        {
            var options = new K8SProviderOption();
            configFunc(options);
            services.Configure(configFunc);
            services.AddKubernetesCore(options.KubeconfigPath, options.UseRelativePaths);
            services.AddSingleton<K8SServiceDiscoveryProvider>();
            services.AddHostedService(p => p.GetRequiredService<K8SServiceDiscoveryProvider>());
            services.AddSingleton<IServiceDiscoveryProvider>(p => p.GetRequiredService<K8SServiceDiscoveryProvider>());
            return services;
        }


        internal static IServiceCollection AddKubernetesCore(this IServiceCollection services,
            string? kubeconfigPath = null, bool useRelativePaths = true)
        {
            var cfg = string.IsNullOrEmpty(kubeconfigPath)
                ? KubernetesClientConfiguration.BuildDefaultConfig()
                : KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfigPath: kubeconfigPath,
                    useRelativePaths: useRelativePaths);
            services.TryAddSingleton<IKubernetes>(new Kubernetes(cfg));
            return services;
        }
    }
}