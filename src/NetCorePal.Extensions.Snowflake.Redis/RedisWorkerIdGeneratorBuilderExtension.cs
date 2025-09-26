using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Snowflake;
using NetCorePal.Extensions.Snowflake.Redis;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class RedisWorkerIdGeneratorBuilderExtension
    {
        public static IServiceCollection AddConsulWorkerIdGenerator(this IServiceCollection services,
            Action<RedisWorkerIdGeneratorOptions> optionSetup)
        {
            optionSetup = optionSetup ?? throw new ArgumentNullException(nameof(optionSetup));
            services.Configure(optionSetup);
            services.AddSingleton<RedisWorkerIdGenerator>();
            services.AddSingleton<IWorkIdGenerator>(p => p.GetRequiredService<RedisWorkerIdGenerator>());
            services.AddHostedService(p => p.GetRequiredService<RedisWorkerIdGenerator>());
            return services;
        }

        public static IServiceCollection AddConsulWorkerIdGeneratorHealthCheck(this IHealthChecksBuilder builder,
            string name = "ConsulWorkerIdGenerator")
        {
            builder.AddCheck<RedisWorkerIdGenerator>(name);
            return builder.Services;
        }
    }
}