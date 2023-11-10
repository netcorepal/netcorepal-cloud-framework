namespace NetCorePal.Extensions.DistributedTransactions;

public interface IIntegrationEventHandlerFilter
{
    Task OnPublishAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next);
}

public delegate Task IntegrationEventHandlerDelegate(IntegrationEventHandlerContext context);