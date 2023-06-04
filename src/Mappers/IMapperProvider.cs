using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Mappers
{

    public interface IMapperProvider
    {
        IMapper<TFrom, TTo> GetMapper<TFrom, TTo>() where TFrom : class where TTo : class;
    }
}
