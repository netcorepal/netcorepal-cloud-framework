using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public interface ISagaEventPublisher
    {
        Task PublishAsync<TSagaEvent>(TSagaEvent integrationEvent, CancellationToken cancellationToken = default) where TSagaEvent : notnull;
    }
}
