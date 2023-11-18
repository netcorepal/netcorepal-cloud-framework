using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Snowflake;
using NetCorePal.Extensions.Snowflake.Consul;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ConsulWorkerIdGeneratorBuilderExtension
    {
        public static IServiceCollection AddConsulWorkerIdGenerator(this IServiceCollection services,
            Action<ConsulWorkerIdGeneratorOptions> optionSetup)
        {
            optionSetup = optionSetup ?? throw new ArgumentNullException(nameof(optionSetup));
            services.Configure(optionSetup);
            services.AddSingleton<ConsulWorkerIdGenerator>();
            services.AddSingleton<IWorkIdGenerator>(p => p.GetRequiredService<ConsulWorkerIdGenerator>());
            services.AddHostedService(p => p.GetRequiredService<ConsulWorkerIdGenerator>());
            return services;
        }

        public static IServiceCollection AddConsulWorkerIdGeneratorHealthCheck(this IHealthChecksBuilder builder,
            string name = "ConsulWorkerIdGenerator")
        {
            builder.AddCheck<ConsulWorkerIdGenerator>(name);
            return builder.Services;
        }
    }
}