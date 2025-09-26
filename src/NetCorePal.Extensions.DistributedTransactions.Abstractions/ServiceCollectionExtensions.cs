using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册IntegrationEventConverter类型
        /// </summary>
        static IIntegrationEventServicesBuilder AddIntegrationEventConverter(
            this IIntegrationEventServicesBuilder builder,
            params Type[] typeFromAssemblies)
        {
            var types = typeFromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t =>
                t is { IsClass: true, IsAbstract: false } && Array.Exists(t.GetInterfaces(), p =>
                    p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IIntegrationEventConverter<,>)));
            foreach (var handler in handlers)
            {
                builder.Services.TryAddScoped(handler);
            }

            return builder;
        }

        /// <summary>
        /// 存储注册的EventHandler类型
        /// </summary>
        /// <exception cref="Exception"></exception>
        static IIntegrationEventServicesBuilder AddIntegrationEventServices(this IServiceCollection services,
            params Type[] typeFromAssemblies)
        {
            var types = typeFromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t =>
                t is { IsClass: true, IsAbstract: false } && Array.Exists(t.GetInterfaces(), p =>
                    p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))).ToList();


            var list = handlers.Select(p =>
            {
                var s = p.GetInterfaces()
                    .First(p => p.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));
                var eventType = s.GenericTypeArguments[0];

                var attr = p.GetCustomAttribute<IntegrationEventConsumerAttribute>();
                var groupName = attr == null ? string.Empty : attr.GroupName;

                return new
                {
                    HandlerType = p,
                    EventType = eventType,
                    GroupName = groupName
                };
            }).GroupBy(p => new { EventType = p.EventType, GroupName = p.GroupName }).ToList();
            var duplicateTypes = list.Find(g => g.Count() > 1);
            if (duplicateTypes != null)
            {
#pragma warning disable S112
                throw new Exception(string.Format(R.Duplicate_IntegrationEvent_Group, duplicateTypes.Key.EventType));
#pragma warning restore S112
            }

            foreach (var handler in handlers)
            {
                services.TryAddScoped(handler);
            }

            services.AddScoped(typeof(IntegrationEventHandlerWrap<,>));
            return new IntegrationEventServicesBuilder(services);
        }

        /// <summary>
        /// 注册所有IntegrationEventHandler类型，并且注册IntegrationEventConverter类型
        /// </summary>
        /// <param name="services"></param>
        /// <param name="typeFromAssemblies">IntegrationEventHandler、IntegrationEventConverter所在的程序集</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IIntegrationEventServicesBuilder AddIntegrationEvents(this IServiceCollection services,
            params Type[] typeFromAssemblies)
        {
            if (typeFromAssemblies.Length == 0)
            {
                throw new ArgumentException(nameof(typeFromAssemblies), R.TypeFromAssemblies_Can_Not_Be_Empty);
            }

            return services.AddIntegrationEventServices(typeFromAssemblies)
                .AddIntegrationEventConverter(typeFromAssemblies);
        }
    }
}