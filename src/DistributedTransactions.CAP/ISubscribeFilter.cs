namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public interface IIngSubscribeFilter
{
    int Order { get; }
    Task OnPublishAsync<TEvent>(IntegrationEventPublishContext<TEvent> context)
        where TEvent : notnull;
}

public delegate Task SubscribeDelegate<TEvent>(IntegrationEventPublishContext<TEvent> context);