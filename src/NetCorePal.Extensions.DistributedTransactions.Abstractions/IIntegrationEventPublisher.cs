namespace NetCorePal.Extensions.DistributedTransactions
{
    
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) where TIntegrationEvent : notnull;
    }
}
