using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public interface ISagaContext<TSagaData> where TSagaData : SagaData
    {
        bool IsComplete();
        bool IsTimeout();
        TSagaData Data { get; }
        ISagaEventPublisher EventPublisher { get; }

        void MarkAsComplete();

        Task RefreshAsync(CancellationToken cancellationToken = default);

        void SetCurrentEvent(string eventName);
    }
}