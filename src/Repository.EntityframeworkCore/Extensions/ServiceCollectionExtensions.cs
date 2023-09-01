using NetCorePal.Extensions.Repository.EntityframeworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using NetCorePal.Extensions.Repository;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Repository.EntityframeworkCore.Behaviors;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        ///// <summary>
        ///// Scan AppDomain.CurrentDomain.GetAssemblies add all IRepository to ServiceCollection
        ///// </summary>
        ///// <param name="services"></param>
        ///// <returns></returns>
        //public static IServiceCollection AddRepositories(this IServiceCollection services)
        //{
        //    return services.AddRepositories(AppDomain.CurrentDomain.GetAssemblies());
        //}

        /// <summary>
        /// Scan assemblies add all IRepository to ServiceCollection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(assembly => assembly.GetTypes());

            foreach (var repositoryType in types.Where(x =>
                         !x.IsGenericType && !x.IsAbstract && x.BaseType != null && x.BaseType.IsGenericType &&
                         (x.BaseType.GetGenericTypeDefinition() == typeof(RepositoryBase<,,>)
                          || x.BaseType.GetGenericTypeDefinition() == typeof(RepositoryBase<,>))))
            {
                var repositoryInterfaceType = repositoryType.GetInterfaces()
                    .FirstOrDefault(x => !x.IsGenericType && x.GetInterfaces()
                        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>)));

                if (repositoryInterfaceType == null)
                    continue;

                services.TryAddScoped(repositoryType);
                services.TryAddScoped(repositoryInterfaceType, p => p.GetRequiredService(repositoryType));
            }

            return services;
        }

        public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
            where TDbContext : EFContext
        {
            services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<TDbContext>());
            services.AddScoped<IEFCoreUnitOfWork>(p => p.GetRequiredService<TDbContext>());
            return services;
        }


        public static MediatRServiceConfiguration AddUnitOfWorkBehaviors(this MediatRServiceConfiguration cfg)
        {
            cfg.AddOpenBehavior(typeof(CommandUnitOfWorkBehavior<,>));
            cfg.AddOpenBehavior(typeof(DomainEventUnitOfWorkBehavior<,>));
            return cfg;
        }
    }
}