using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.DistributedTransactions
{
    public abstract class IntegrationEventPublisher : IIntegrationEventPublisher
    {
        readonly IntegrationEventPublishDelegate _next;

        protected IntegrationEventPublisher(
            IEnumerable<IIntegrationEventPublisherFilter> publisherFilters)
        {
            IntegrationEventPublishDelegate next = PublishAsync;
            foreach (var filter in publisherFilters.Reverse())
            {
                var current = next;
                next = context => filter.PublishAsync(context, current);
            }

            _next = next;
        }

        protected abstract Task PublishAsync(IntegrationEventPublishContext context);

        public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default) where TIntegrationEvent : notnull
        {
            var context =
                new IntegrationEventPublishContext<TIntegrationEvent>(integrationEvent,
                    new Dictionary<string, string?>(),
                    cancellationToken);
            await _next(context);
        }
    }
}