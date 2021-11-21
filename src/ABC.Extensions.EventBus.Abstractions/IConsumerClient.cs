using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Extensions.EventBus
{
    public interface IConsumerClient
    {
        Task SubscribeAsync(string subscribeName, string eventName, Action<EventData> action);

        Task UnSubscribeAsync(string subscribeName, string eventName);
    }



}
