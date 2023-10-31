using DotNetCore.CAP;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.DistributedTransactions.CAP
{
    public class CapIntegrationEventPublisher : IIntegrationEventPublisher
    {
        readonly ICapPublisher _capPublisher;

        readonly IEnumerable<IPublisherFilter> _publisherFilters;

        public CapIntegrationEventPublisher(ICapPublisher capPublisher,
            IEnumerable<IPublisherFilter> publisherFilters)
        {
            _capPublisher = capPublisher;
            _publisherFilters = publisherFilters.OrderBy(p => p.Order).ToList();
        }

        public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default) where TIntegrationEvent : notnull
        {
            var context =
                new EventPublishContext<TIntegrationEvent>(integrationEvent, new Dictionary<string, string?>());
            foreach (var filter in _publisherFilters)
            {
                await filter.OnPublishAsync(context, cancellationToken);
            }

            await _capPublisher.PublishAsync(name: nameof(TIntegrationEvent),
                contentObj: context.Data,
                headers: context.Headers,
                cancellationToken: cancellationToken);
        }
    }
}