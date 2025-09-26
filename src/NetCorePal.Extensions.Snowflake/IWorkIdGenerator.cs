using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Snowflake
{
    public interface IWorkIdGenerator
    {
        long GetId();
    }
}
