using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Mappers
{
    public interface IMapper<in TFrom, out TTo> where TFrom : class where TTo : class
    {
        TTo To(TFrom from);
    }
}
