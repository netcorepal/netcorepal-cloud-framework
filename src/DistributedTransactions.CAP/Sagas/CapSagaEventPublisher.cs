using DotNetCore.CAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    internal class CapSagaEventPublisher : ISagaEventPublisher
    {
        readonly ICapPublisher _capPublisher;
        private readonly IEnumerable<IIntegrationEventPublisherFilter> _publisherFilters;

        public CapSagaEventPublisher(ICapPublisher capPublisher,
            IEnumerable<IIntegrationEventPublisherFilter> publisherFilters)
        {
            _capPublisher = capPublisher;
            _publisherFilters = publisherFilters.ToList();
        }

        public async Task PublishAsync<TSagaEvent>(TSagaEvent integrationEvent,
            CancellationToken cancellationToken = default)
            where TSagaEvent : notnull
        {
            var context =
                new IntegrationEventPublishContext<TSagaEvent>(integrationEvent, new Dictionary<string, string?>());
            foreach (var filter in _publisherFilters)
            {
                throw new NotImplementedException();
                //await filter.OnPublishAsync(context);
            }

            await _capPublisher.PublishAsync(name: typeof(TSagaEvent).Name, contentObj: integrationEvent,
                cancellationToken: cancellationToken);
        }
    }
}