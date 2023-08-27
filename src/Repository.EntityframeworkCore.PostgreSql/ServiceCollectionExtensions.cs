using NetCorePal.Extensions.Repository.EntityframeworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Repository.EntityframeworkCore.PostgreSql;

namespace NetCorePal.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scan AppDomain.CurrentDomain.GetAssemblies add all IRepository to ServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPostgreSqlTransactionHandler(this IServiceCollection services)
        {
            services.TryAddScoped<IPublisherTransactionHandler, CapPostgreSqlPublisherTransactionHandler>();
            return services;
        }
    }
}
