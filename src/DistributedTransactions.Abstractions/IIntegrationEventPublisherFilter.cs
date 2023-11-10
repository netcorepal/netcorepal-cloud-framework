namespace NetCorePal.Extensions.DistributedTransactions;

public interface IIntegrationEventPublisherFilter
{
    Task OnPublishAsync(IntegrationEventPublishContext context, PublisherDelegate next);
}

public delegate Task PublisherDelegate(IntegrationEventPublishContext context);