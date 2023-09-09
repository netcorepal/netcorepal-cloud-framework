namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public interface ISagaEvent
{
    Guid SagaId { get; }
}