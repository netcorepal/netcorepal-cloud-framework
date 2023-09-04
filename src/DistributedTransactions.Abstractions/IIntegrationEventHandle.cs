using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions
{



    public interface IIntegrationEventHandle<TIntegrationEvent> where TIntegrationEvent : notnull
    {
        public Task HandleAsync(TIntegrationEvent eventData,CancellationToken cancellationToken = default);
    }
}
