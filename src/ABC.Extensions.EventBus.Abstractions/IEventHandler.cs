using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Extensions.EventBus
{
    public interface IEventHandler
    {
    }


    public interface IEventHandler<in TEventData> : IEventHandler where TEventData : class
    {
        Task HandleAsync(TEventData eventData);
    }

}
