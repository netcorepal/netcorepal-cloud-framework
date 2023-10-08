namespace NetCorePal.Extensions.DistributedTransactions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> where TIntegrationEvent : notnull
    {
        public Task HandleAsync(TIntegrationEvent eventData, CancellationToken cancellationToken = default);
    }
}
