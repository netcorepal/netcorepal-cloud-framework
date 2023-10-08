using DotNetCore.CAP;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Repository;

namespace NetCorePal.Web.Application.Sagas
{

    public class CreateOrderSagaData : SagaData<long>
    {
        public CreateOrderSagaData()
        {
            this.SagaId = System.Guid.NewGuid();
        }

    }


    public class SagaEvent(Guid sagaId) : ISagaEvent
    {
        public Guid SagaId { get; } = sagaId;
    }



    public class CreateOrderSaga(ISagaContext<CreateOrderSagaData> context) : Saga<CreateOrderSagaData, long>(context),
        ISagaEventHandler<SagaEvent>
    {
        public Task HandleAsync(SagaEvent eventData, CancellationToken cancellationToken = default)
        {
            Context.MarkAsComplete();
            return Task.CompletedTask;
        }

        public override async Task OnStart(CreateOrderSagaData data, CancellationToken cancellationToken = default)
        {
            await Context.EventPublisher.PublishAsync(new SagaEvent(Context.Data.SagaId), cancellationToken);
        }
    }

}
