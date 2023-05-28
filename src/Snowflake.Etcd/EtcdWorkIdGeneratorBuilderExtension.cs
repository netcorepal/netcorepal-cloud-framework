using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NetCorePal.Extensions.Snowflake.Etcd
{
    public static class EtcdWorkIdGeneratorBuilderExtension
    {
        public static IServiceCollection AddEtcd(this IServiceCollection services, Action<EtcdOptions> optionSetup)
        {
            optionSetup = optionSetup ?? throw new ArgumentNullException(nameof(optionSetup));

            services.Configure(optionSetup);
            services.AddSingleton<IWorkIdGenerator, EtcdWorkIdGenerator>();
            services.AddHostedService<EtcdBackgroundService>();
            return services;
        }
    }
}
