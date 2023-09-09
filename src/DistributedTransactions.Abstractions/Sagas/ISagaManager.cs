namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public interface ISagaManager
{
    Task SendAsync<TSaga, TSagaData>(TSagaData sagaData, CancellationToken cancellationToken = default)
        where TSaga : Saga<TSagaData>
        where TSagaData : SagaData;

    Task<TResult?> SendAsync<TSaga, TSagaData, TResult>(TSagaData sagaData, CancellationToken cancellationToken = default)
        where TSaga : Saga<TSagaData, TResult>
        where TSagaData : SagaData<TResult>;
}