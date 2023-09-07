using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 存储注册的EventHandler类型
        /// </summary>
        public static IServiceCollection AddSagaRepository<TDbContext>(this IServiceCollection services,
            params Type[] typefromAssemblies)
            where TDbContext : EFContext
        {
            services.TryAddScoped<SagaRepository<TDbContext>>();
            
            //services.TryAddScoped<EFSagaContext<TDbContext,t>>();
            

            return services;
        }
    }
}