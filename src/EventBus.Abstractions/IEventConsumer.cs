using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.EventBus
{
    public interface IEventConsumer
    {
        Task SubscribeAsync<T>(string consumerName, string eventName, Func<T, IServiceScope, Task> func);
    }
}
