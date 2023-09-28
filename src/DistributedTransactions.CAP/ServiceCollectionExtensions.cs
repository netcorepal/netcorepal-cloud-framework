using DotNetCore.CAP;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册所有EventHandler类型
        /// </summary>
        public static IServiceCollection AddAllCAPEventHanders(this IServiceCollection services,
            params Type[] typefromAssemblies)
        {
            var types = typefromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t =>
                t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(ICapSubscribe)));

            foreach (var handler in handlers)
            {
                services.TryAddTransient(handler);
            }

            services.AddAllIIntegrationEventHandlers(typefromAssemblies);
            return services;
        }

        /// <summary>
        /// 注册SagaEventPublisher
        /// </summary>
        public static IServiceCollection AddCAPSagaEventPublisher(this IServiceCollection services)
        {
            services.TryAddSingleton<ISagaEventPublisher, CapSagaEventPublisher>();
            return services;
        }

        /// <summary>
        /// 注册Saga相关服务
        /// </summary>
        public static IServiceCollection AddSagas<TDbContext>(this IServiceCollection services,
            params Type[] typeFromAssemblies)
            where TDbContext : DbContext, IUnitOfWork, ITransactionUnitOfWork
        {
            services.TryAddScoped<SagaRepository<TDbContext>>();
            services.TryAddScoped<ISagaManager, SagaManager>();

            var types = typeFromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes()).ToList();

            var sagas = types.Where(t =>
                t is { IsClass: true, IsAbstract: false } && t.IsGenericSubclassOf(typeof(Saga<>)));
            foreach (var saga in sagas)
            {
                services.TryAddScoped(saga);

                var args = saga.BaseType!.GetGenericArguments();

                if (args.Length == 1)
                {
                    var sender = typeof(SagaSender<,>).MakeGenericType(saga, args[0]);
                    services.TryAddScoped(sender);
                }
                else
                {
                    var sender = typeof(SagaSender<,,>).MakeGenericType(saga, args[0], args[1]);
                    services.TryAddScoped(sender);
                }
            }

            var sagaDataTypes = types.Where(t =>
                t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(SagaData)));
            foreach (var sagaDataType in sagaDataTypes)
            {
                var sagaInterface = typeof(ISagaContext<>).MakeGenericType(sagaDataType);
                var sagaContextType = typeof(SagaContext<,>).MakeGenericType(typeof(TDbContext), sagaDataType);
                services.TryAddScoped(sagaInterface, sagaContextType);
                services.TryAddTransient(
                    typeof(IRequestHandler<>).MakeGenericType(
                        typeof(CreateSagaCommand<>).MakeGenericType(sagaDataType)),
                    typeof(CreateSagaCommandHandler<,>).MakeGenericType(typeof(TDbContext), sagaDataType));
            }

            return services;
        }


        public static bool IsGenericSubclassOf(this Type type, Type superType)
        {
            if (type.BaseType != null
                && !type.BaseType.Equals(typeof(object))
                && type.BaseType.IsGenericType)
            {
                if (type.BaseType.GetGenericTypeDefinition().Equals(superType))
                {
                    return true;
                }

                return type.BaseType.IsGenericSubclassOf(superType);
            }

            return false;
        }
    }
}