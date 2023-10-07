using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.Mappers;
using System.Reflection;
namespace NetCorePal.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMapperPrivider(this IServiceCollection services)
        {
            services.TryAddSingleton<IMapperProvider, MapperProvider>();
            return services;
        }


        public static IServiceCollection AddMapperPrivider(this IServiceCollection services, Assembly assembly)
        {
            services.TryAddSingleton<IMapperProvider, MapperProvider>();
            var types = assembly.GetTypes();
            foreach (var mapperType in types.Where(x => !x.IsGenericType && !x.IsAbstract && Array.Exists(x.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>))))
            {
                var mapperInterfaceType = mapperType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>));

                if (mapperInterfaceType == null)
                    continue;

                services.TryAddSingleton(mapperInterfaceType, mapperType);
            }
            return services;
        }

    }
}
