namespace NetCorePal.Extensions.DistributedTransactions
{
    
    public interface IntegrationEventPublisher
    {
        Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default) where TIntegrationEvent : notnull;
    }
}
