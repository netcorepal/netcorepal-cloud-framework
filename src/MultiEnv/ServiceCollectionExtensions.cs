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
        public static IServiceCollection AddEnvServiceSelector(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IServiceSelector, EnvServiceSelector>());
            return services;
        }

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
    }
}