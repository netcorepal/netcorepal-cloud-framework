using DotNetCore.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    internal class CapSagaEventPublisher : ISagaEventPublisher
    {
#pragma warning disable S4487
        readonly ICapPublisher _capPublisher;
#pragma warning restore S4487
#pragma warning disable S4487
        private readonly IEnumerable<IIntegrationEventPublisherFilter> _publisherFilters;
#pragma warning restore S4487

        public CapSagaEventPublisher(ICapPublisher capPublisher,
            IEnumerable<IIntegrationEventPublisherFilter> publisherFilters)
        {
            _capPublisher = capPublisher;
            _publisherFilters = publisherFilters.ToList();
        }

        public Task PublishAsync<TSagaEvent>(TSagaEvent integrationEvent,
            CancellationToken cancellationToken = default)
            where TSagaEvent : notnull
        {
            
            throw new NotImplementedException();
            // var context =
#pragma warning disable S125
            //     new IntegrationEventPublishContext<TSagaEvent>(integrationEvent, new Dictionary<string, string?>());
#pragma warning restore S125
            // foreach (var filter in _publisherFilters)
            // {
            //     throw new NotImplementedException();
            // }
            //
            // await _capPublisher.PublishAsync(name: typeof(TSagaEvent).Name, contentObj: integrationEvent,
            //     cancellationToken: cancellationToken);
        }
    }
}