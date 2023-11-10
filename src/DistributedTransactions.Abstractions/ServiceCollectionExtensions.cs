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
        public static IIntegrationEventServicesBuilder AddIntegrationEventServices(this IServiceCollection services,
            params Type[] typeFromAssemblies)
        {
            var types = typeFromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t =>
                t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IIntegrationEventHandler<>)));

            foreach (var handler in handlers)
            {
                services.TryAddScoped(handler);
            }

            services.AddScoped(typeof(IntegrationEventHandlerWrap<,>));
            return new IntegrationEventServicesBuilder(services);
        }

        public static IIntegrationEventServicesBuilder AddTransactionIntegrationEventHandlerFilter(
            this IIntegrationEventServicesBuilder builder)
        {
            builder.Services.AddScoped<IIntegrationEventHandlerFilter, TransactionIntegrationEventHandlerFilter>();
            return builder;
        }
    }
}