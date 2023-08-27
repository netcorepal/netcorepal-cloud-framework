using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Repository.EntityframeworkCore;
using NetCorePal.Extensions.Repository.EntityframeworkCore.MySql;

namespace NetCorePal.Extensions
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
