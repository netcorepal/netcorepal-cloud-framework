namespace NetCorePal.Extensions.DistributedTransactions;

public interface IIntegrationEventPublisherFilter
{
    Task PublishAsync(IntegrationEventPublishContext context, IntegrationEventPublishDelegate next);
}

public delegate Task IntegrationEventPublishDelegate(IntegrationEventPublishContext context);