using DotNetCore.CAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas.CAP
{
    internal class CAPSagaEventPublisher : ISagaEventPublisher
    {
        readonly ICapPublisher _capPublisher;
        public CAPSagaEventPublisher(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public Task PublishAsync<TSagaEvent>(TSagaEvent integrationEvent, CancellationToken cancellationToken = default) where TSagaEvent : notnull
        {

            return _capPublisher.PublishAsync(name: typeof(TSagaEvent).Name, contentObj: integrationEvent, cancellationToken: cancellationToken);
        }
    }
}
