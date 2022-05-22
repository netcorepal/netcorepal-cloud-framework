using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.EventBus
{
    /// <summary>
    /// EventHandler注册方式
    /// </summary>
    public interface IEventHandlerRegistrator
    {
        IEnumerable<Type> GetAllEventHandlerTypes();
    }
}
