using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// 存储注册的EventHandler类型
        /// </summary>
        internal readonly static HashSet<Type> handlerTypes = new HashSet<Type>();
        public static IServiceCollection AddAllIIntegrationEventHanders(this IServiceCollection services, params Type[] typefromAssemblies)
        {
            var types = typefromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IIntegrationEventHandle<>)));

            foreach (var handler in handlers)
            {
                if (handler.IsClass && !handler.IsAbstract && handler.GetInterfaces().Any(t => t.GetGenericTypeDefinition() == typeof(IIntegrationEventHandle<>)))
                {
                    services.AddScoped(handler);
                    handlerTypes.Add(handler);
                }
            }
            return services;
        }
    }
}
