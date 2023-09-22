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
        private readonly IEnumerable<IPublisherFilter> _publisherFilters;

        public CapSagaEventPublisher(ICapPublisher capPublisher, IEnumerable<IPublisherFilter> publisherFilters)
        {
            _capPublisher = capPublisher;
            _publisherFilters = publisherFilters.OrderBy(p => p.Order).ToList();
        }

        public async Task PublishAsync<TSagaEvent>(TSagaEvent integrationEvent,
            CancellationToken cancellationToken = default)
            where TSagaEvent : notnull
        {
            var context =
                new EventPublishContext<TSagaEvent>(integrationEvent, new Dictionary<string, string?>());
            foreach (var filter in _publisherFilters)
            {
                await filter.OnPublishAsync(context);
            }

            await _capPublisher.PublishAsync(name: typeof(TSagaEvent).Name, contentObj: integrationEvent,
                cancellationToken: cancellationToken);
        }
    }
}