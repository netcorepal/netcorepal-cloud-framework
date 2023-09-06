using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public interface ISagaContext<TSagaData> where TSagaData : SagaData
    {
        TSagaData Data { get; }
        ISagaEventPublisher EventPublisher { get; }

        void MarkAsComplete();

        void SetCurrentEvent(string eventName);
    }
}
