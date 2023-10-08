namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public interface ISagaEventPublisher
    {
        Task PublishAsync<TSagaEvent>(TSagaEvent integrationEvent, CancellationToken cancellationToken = default) where TSagaEvent : notnull;
    }
}
