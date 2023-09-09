using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

// ReSharper disable once CheckNamespace
namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 存储注册的EventHandler类型
        /// </summary>
        public static IServiceCollection AddSagas<TDbContext>(this IServiceCollection services,
            params Type[] typeFromAssemblies)
            where TDbContext : EFContext
        {
            services.TryAddScoped<SagaRepository<TDbContext>>();
            services.TryAddSingleton<ISagaManager, SagaManager>();
            services.AddHostedService<SagaHostedService<TDbContext>>();

            var types = typeFromAssemblies.Select(p => p.Assembly).SelectMany(assembly => assembly.GetTypes()).ToList();

            var sagas = types.Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(Saga<>)));
            foreach (var saga in sagas)
            {
                services.TryAddScoped(saga);
            }

            var sagaDataTypes = types.Where(t =>
                t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(SagaData)));
            foreach (var sagaDataType in sagaDataTypes)
            {
                var sagaContextType =
                    Type.MakeGenericSignatureType(typeof(SagaContext<,>), typeof(TDbContext), sagaDataType);
                services.TryAddScoped(sagaContextType);
            }

            return services;
        }
    }
}