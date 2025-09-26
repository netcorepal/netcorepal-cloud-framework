using System.Collections.Concurrent;
using System.Reflection;

namespace NetCorePal.Extensions.Mappers
{
    public static class MapperProviderExtensions
    {
        public static TTo MapTo<TFrom, TTo>(this IMapperProvider mapperFactory, TFrom from) where TTo : class where TFrom : class
        {
            return mapperFactory.GetMapper<TFrom, TTo>().To(from);
        }

        public static TTo MapTo<TTo>(this object from, IMapperProvider provider) where TTo : class
        {
            var mapperType = mapperTypeCache.GetOrAdd(from.GetType(), typeof(IMapper<,>).MakeGenericType(from.GetType(), typeof(TTo)));
            var mapper = provider.GetMapper(mapperType);
            var methodInfo = methodInfoCache.GetOrAdd(mapperType, mapperType.GetMethod("To") ?? throw new Exception("未找到Mapper的To方法"));
            var to = methodInfo.Invoke(mapper, new object[] { from });
            if (to != null)
            {
                return (TTo)to;
            }
#pragma warning disable S112 // General exceptions should never be thrown
            throw new Exception("转换对象为空");
#pragma warning restore S112 // General exceptions should never be thrown
        }

        public static TTo MapTo<TFrom, TTo>(this TFrom from, IMapperProvider provider) where TTo : class where TFrom : class
        {
            return provider.GetMapper<TFrom, TTo>().To(from);
        }



        static readonly ConcurrentDictionary<Type, Type> mapperTypeCache = new();
        static readonly ConcurrentDictionary<Type, MethodInfo> methodInfoCache = new();


    }
}
