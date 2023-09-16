using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Context;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加环境上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEnvContext(this IServiceCollection services)
        {
            services.AddContextCore();
            services.TryAddSingleton<IContextCarrierHandler, EnvContextCarrierHandler>();
            services.TryAddSingleton<IContextSourceHandler, EnvContextSourceHandler>();
            return services;
        }


        /// <summary>
        /// 添加租户上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTenantContext(this IServiceCollection services)
        {
            services.AddContextCore();
            services.TryAddSingleton<IContextCarrierHandler, TenantContextCarrierHandler>();
            services.TryAddSingleton<IContextSourceHandler, TenantContextSourceHandler>();
            return services;
        }
    }
}