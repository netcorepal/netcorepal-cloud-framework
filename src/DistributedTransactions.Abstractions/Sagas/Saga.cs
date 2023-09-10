namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public abstract class Saga<TSagaData> where TSagaData : SagaData
{
    protected Saga(ISagaContext<TSagaData> context)
    {
        Context = context;
    }

    public ISagaContext<TSagaData> Context { get; }
    public abstract Task OnStart(TSagaData data, CancellationToken cancellationToken = default);
}

public abstract class Saga<TSagaData, TResult> : Saga<TSagaData> where TSagaData : SagaData<TResult>
{
    protected Saga(ISagaContext<TSagaData> context) : base(context)
    {
    }
}