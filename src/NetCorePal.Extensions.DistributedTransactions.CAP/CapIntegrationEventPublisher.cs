using DotNetCore.CAP;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.DistributedTransactions.CAP
{
    public sealed class CapIntegrationEventPublisher : IntegrationEventPublisher
    {
        readonly ICapPublisher _capPublisher;

        public CapIntegrationEventPublisher(ICapPublisher capPublisher,
            IEnumerable<IIntegrationEventPublisherFilter> publisherFilters) : base(publisherFilters)
        {
            _capPublisher = capPublisher;
        }

        protected override Task PublishAsync(IntegrationEventPublishContext context)
        {
            return _capPublisher.PublishAsync(name: context.Data.GetType().Name,
                contentObj: context.Data,
                headers: context.Headers,
                cancellationToken: context.CancellationToken);
        }
    }
}