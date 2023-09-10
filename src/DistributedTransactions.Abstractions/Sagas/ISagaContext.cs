using NetCorePal.Extensions.Repository;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public interface ISagaContext<TSagaData> where TSagaData : SagaData
    {
        bool IsComplete();
        bool IsTimeout();
        TSagaData Data { get; }
        ISagaEventPublisher EventPublisher { get; }

        void MarkAsComplete();

        Task RefreshAsync(CancellationToken cancellationToken = default);

        Task InitAsync(Guid sagaId, CancellationToken cancellationToken = default);

        Task StartNewSagaAsync(TSagaData sagaData, CancellationToken cancellationToken = default);
    }
}