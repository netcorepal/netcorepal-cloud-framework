namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public interface ISagaEventHandler<in TEvent> where TEvent : notnull, ISagaEvent
    {
        Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
    }
}