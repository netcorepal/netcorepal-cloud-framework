using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// 存储注册的EventHandler类型
        /// </summary>
        public static IServiceCollection AddAllIIntegrationEventHandlers(this IServiceCollection services, params Type[] typefromAssemblies)
        {
            var types = typefromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IIntegrationEventHandler<>)));

            foreach (var handler in handlers)
            {
                services.TryAddScoped(handler);
            }
            return services;
        }
    }
}
