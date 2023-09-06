using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public interface ISagaEventHandler<TEvent> where TEvent : notnull
    {
        Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
    }
}
