using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.EventBus
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string eventName, T eventData, CancellationToken cancellationToken = default) where T : class;
    }
}
