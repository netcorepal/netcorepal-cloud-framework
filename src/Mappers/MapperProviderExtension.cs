using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Mappers
{
    public static class MapperProviderExtension
    {
        public static TTo MapTo<TFrom, TTo>(this IMapperProvider mapperFactory, TFrom from) where TTo : class where TFrom : class
        {
            return mapperFactory.GetMapper<TFrom, TTo>().To(from);
        }


        public static TTo MapTo<TFrom, TTo>(this TFrom from, IMapperProvider provider) where TTo : class where TFrom : class
        {
            return provider.GetMapper<TFrom, TTo>().To(from);
        }
    }
}
