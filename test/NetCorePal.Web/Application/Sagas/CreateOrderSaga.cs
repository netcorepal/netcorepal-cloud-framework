using NetCorePal.Extensions.DistributedTransactions.Sagas;

namespace NetCorePal.Web.Application.Sagas
{

    public class CreateOrderSagaData : SagaData<long>
    {
        public CreateOrderSagaData()
        {
            this.SagaId = System.Guid.NewGuid();
        }

    }


    public class SagaEvent : ISagaEvent
    {
        public SagaEvent(Guid sagaId)
        {
            SagaId = sagaId;
        }

        public Guid SagaId { get; }
    }



    public class CreateOrderSaga : Saga<CreateOrderSagaData, long>
    {
        public CreateOrderSaga(ISagaContext<CreateOrderSagaData> context) : base(context)
        {
        }

        public override async Task OnStart(CreateOrderSagaData data, CancellationToken cancellationToken = default)
        {
            await Context.EventPublisher.PublishAsync(new SagaEvent(Context.Data.SagaId), cancellationToken);
        }
    }
}
