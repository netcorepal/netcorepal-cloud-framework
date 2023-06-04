using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
namespace NetCorePal.Extensions.Mappers
{
    public class MapperProvider : IMapperProvider
    {
        readonly IServiceProvider _serviceProvider;
        public MapperProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMapper<TFrom, TTo> GetMapper<TFrom, TTo>()
            where TFrom : class
            where TTo : class
        {
            return _serviceProvider.GetRequiredService<IMapper<TFrom, TTo>>();
        }
    }
}
