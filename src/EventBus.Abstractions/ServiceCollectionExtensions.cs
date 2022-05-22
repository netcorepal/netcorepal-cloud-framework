using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.EventBus;
using Microsoft.Extensions.DependencyInjection;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// 存储注册的EventHandler类型
        /// </summary>
        internal readonly static HashSet<Type> handlerTypes = new HashSet<Type>();
        public static IServiceCollection AddAllEventHanders(this IServiceCollection services, params Type[] typefromAssemblies)
        {
            var types = typefromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t => t.GetInterfaces().Contains(typeof(IEventHandler)));

            foreach (var handler in handlers)
            {
                if (handler.IsClass && !handler.IsAbstract && handler.GetInterfaces().Any(t => t.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
                {
                    services.AddScoped(handler);
                    handlerTypes.Add(handler);
                }
            }
            return services;
        }





        public static IServiceCollection AddDefaultEventBusServer(this IServiceCollection services)
        {
            return services.AddHostedService<DefaultEventBusServer>();
        }
    }
}
