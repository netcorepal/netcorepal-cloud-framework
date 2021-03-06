using NetCorePal.Extensions.Repository.EntityframeworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using NetCorePal.Extensions.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scan AppDomain.CurrentDomain.GetAssemblies add all IRepository to ServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services.AddRepositories(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Scan assemblies add all IRepository to ServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(assembly => assembly.GetTypes());

            foreach (var repositoryType in types.Where(x => !x.IsGenericType && !x.IsAbstract && x.BaseType != null && x.BaseType.IsGenericType &&
                                                            (x.BaseType.GetGenericTypeDefinition() == typeof(RepositoryBase<,,>)
                                                             || x.BaseType.GetGenericTypeDefinition() == typeof(RepositoryBase<,>))))
            {
                var repositoryInterfaceType = repositoryType.GetInterfaces()
                    .FirstOrDefault(x => !x.IsGenericType && x.GetInterfaces()
                                             .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>)));

                if (repositoryInterfaceType == null)
                    continue;

                services.TryAddScoped(repositoryInterfaceType, repositoryType);
            }

            return services;
        }
    }
}
