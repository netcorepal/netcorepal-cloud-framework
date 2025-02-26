using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.ServiceDiscovery;
using NetCorePal.Extensions.MultiEnv;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加多环境支持
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IMultiEnvServicesBuilder AddMultiEnv(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<EnvOptions>().Bind(configuration)
                .Validate(option => !string.IsNullOrEmpty(option.ServiceName), "EnvOptions.ServiceName is required");
            services.AddSingleton<IIntegrationEventHandlerFilter, EnvIntegrationEventHandlerFilter>();
            return new MultiEnvServicesBuilder(services);
        }

        /// <summary>
        /// 添加多环境支持
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IMultiEnvServicesBuilder AddMultiEnv(
            this IServiceCollection services, Action<EnvOptions> configure)
        {
            services.AddOptions<EnvOptions>().Configure(configure)
                .Validate(option => !string.IsNullOrEmpty(option.ServiceName), "EnvOptions.ServiceName is required");
            services.AddSingleton<IIntegrationEventHandlerFilter, EnvIntegrationEventHandlerFilter>();
            return new MultiEnvServicesBuilder(services);
        }
    }
}