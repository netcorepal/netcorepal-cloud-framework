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
        //废弃该方法，改用public static IMultiEnvServicesBuilder AddEnvServiceSelector(this IMultiEnvServicesBuilder builder) 
        [Obsolete]
        public static IServiceCollection AddEnvServiceSelector(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IServiceSelector, EnvServiceSelector>());
            return services;
        }

        /// <summary>
        /// 该方法已废弃，请使用IMultiEnvServicesBuilder的扩展方法AddIntegrationEventServices
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IIntegrationEventServicesBuilder AddEnvIntegrationFilters(
            this IIntegrationEventServicesBuilder builder, Action<EnvOptions> configure)
        {
            builder.Services.AddOptions<EnvOptions>().Configure(configure)
                .Validate(option=>!string.IsNullOrEmpty(option.ServiceName), "EnvOptions.ServiceName is required");
            builder.Services.AddSingleton<IIntegrationEventHandlerFilter, EnvIntegrationEventHandlerFilter>();
            return builder;
        }

        public static IIntegrationEventServicesBuilder AddEnvIntegrationFilters(
            this IIntegrationEventServicesBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddOptions<EnvOptions>().Bind(configuration)
                .Validate(option=>!string.IsNullOrEmpty(option.ServiceName), "EnvOptions.ServiceName is required");
            builder.Services.AddSingleton<IIntegrationEventHandlerFilter, EnvIntegrationEventHandlerFilter>();
            return builder;
        }

        public static IMultiEnvServicesBuilder AddMultiEnv(
            this IServiceCollection services, IConfigurationSection configuration)
        {
            services.Configure<EnvOptions>(options => configuration.Bind(options));
            return new MultiEnvServicesBuilder(services);
        }
    }
}