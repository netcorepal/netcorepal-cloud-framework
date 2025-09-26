namespace NetCorePal.Extensions.DistributedTransactions;

public interface IIntegrationEventHandlerFilter
{
    Task HandleAsync(IntegrationEventHandlerContext context, IntegrationEventHandlerDelegate next);
}

public delegate Task IntegrationEventHandlerDelegate(IntegrationEventHandlerContext context);