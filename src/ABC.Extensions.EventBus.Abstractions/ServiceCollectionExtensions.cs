using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABC.Extensions.EventBus;
using Microsoft.Extensions.DependencyInjection;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllEventHanders(this IServiceCollection services, params Type[] typefromAssemblies)
        {
            var types = typefromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t => t.GetInterfaces().Contains(typeof(IEventHandler)));
            foreach (var handler in handlers)
            {
                var i = handler.GetInterfaces().FirstOrDefault(t => t.IsGenericType);
                if (i != null)
                {
                    services.AddScoped(handler);
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
