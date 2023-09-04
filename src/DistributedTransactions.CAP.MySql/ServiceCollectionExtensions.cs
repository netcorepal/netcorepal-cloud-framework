using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions.CAP.MySql;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scan AppDomain.CurrentDomain.GetAssemblies add all IRepository to ServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMySqlTransactionHandler(this IServiceCollection services)
        {
            services.TryAddScoped<IPublisherTransactionHandler, CapMySqlPublisherTransactionHandler>();
            return services;
        }


    }
}
