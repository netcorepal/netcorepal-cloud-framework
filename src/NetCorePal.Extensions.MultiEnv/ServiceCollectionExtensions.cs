using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.CAP;
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
            return services.AddMultiEnv(configuration.Bind);
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
            var optionBuilder = services.AddOptions<EnvOptions>().Configure(configure)
                .Validate(option => !string.IsNullOrEmpty(option.ServiceName), "EnvOptions.ServiceName is required");
            optionBuilder.Configure(options =>
            {
                // configure CAP group name
                if (!string.IsNullOrEmpty(options.ServiceEnv))
                {
                    EnvCapSubscribeAttribute.ServiceEnv = options.ServiceEnv;
                    services.Configure<CapOptions>(p =>
                    {
                        p.DefaultGroupName = p.DefaultGroupName + "." + options.ServiceEnv;
                    });
                }
            });
            services.AddSingleton<IIntegrationEventHandlerFilter, EnvIntegrationEventHandlerFilter>();
            return new MultiEnvServicesBuilder(services);
        }
    }
}