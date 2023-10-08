using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaStartEvent<TSagaData> : ISagaEvent, IDomainEvent
    where TSagaData : notnull, SagaData
{
    public SagaStartEvent(TSagaData sagaData)
    {
        SagaData = sagaData;
    }

    public TSagaData SagaData { get; init; }
    public Guid SagaId => SagaData.SagaId;
}