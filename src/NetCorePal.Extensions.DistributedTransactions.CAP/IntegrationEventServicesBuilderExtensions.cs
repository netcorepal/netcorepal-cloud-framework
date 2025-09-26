using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.CAP;
using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using CapBuilder = NetCorePal.Extensions.DistributedTransactions.CAP.CapBuilder;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class IntegrationEventServicesBuilderExtensions
    {
        /// <summary>
        /// 注册所有EventHandler类型
        /// </summary>
        public static ICapBuilder RegisterServicesFromAssemblies(this ICapBuilder builder,
            params Type[] typefromAssemblies)
        {
            var types = typefromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes());
            var handlers = types.Where(t =>
                t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(ICapSubscribe)));

            foreach (var handler in handlers)
            {
                builder.Services.TryAddTransient(handler);
            }

            builder.Services.AddSingleton<IIntegrationEventPublisher, CapIntegrationEventPublisher>();
            return builder;
        }
        static ICapBuilder UseCapUnitOfWork<TDbContext>(this ICapBuilder builder)
            where TDbContext : IUnitOfWork, ITransactionUnitOfWork
        {
            builder.Services.Replace(ServiceDescriptor.Scoped<ITransactionUnitOfWork>(p =>
                new CapTransactionUnitOfWork(p.GetRequiredService<TDbContext>(),
                    p.GetRequiredService<ICapPublisher>(),
                    p.GetRequiredService<ICapTransactionFactory>())));
            return builder;
        }


        /// <summary>
        /// 使用Cap作为集成事件的技术实现
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <typeparam name="TDbContext"></typeparam>
        /// <returns></returns>
        public static IIntegrationEventServicesBuilder UseCap<TDbContext>(this IIntegrationEventServicesBuilder builder,
            Action<ICapBuilder> configure)
            where TDbContext : IUnitOfWork, ITransactionUnitOfWork
        {
            var capBuilder = new CapBuilder(builder.Services);
            capBuilder.UseCapUnitOfWork<TDbContext>();
            configure(capBuilder);
            return builder;
        }

        internal static bool IsGenericSubclassOf(this Type type, Type superType)
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