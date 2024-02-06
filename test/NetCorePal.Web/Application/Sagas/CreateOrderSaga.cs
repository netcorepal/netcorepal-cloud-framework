using DotNetCore.CAP;
using NetCorePal.Extensions.DistributedTransactions.Sagas;
using NetCorePal.Extensions.Repository;

namespace NetCorePal.Web.Application.Sagas
{

    /// <summary>
    /// 
    /// </summary>
    public class CreateOrderSagaData : SagaData<long>
    {
        /// <summary>
        /// 
        /// </summary>
        public CreateOrderSagaData()
        {
            this.SagaId = System.Guid.NewGuid();
        }

    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sagaId"></param>
    public class SagaEvent(Guid sagaId) : ISagaEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid SagaId { get; } = sagaId;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public class CreateOrderSaga(ISagaContext<CreateOrderSagaData> context) : Saga<CreateOrderSagaData, long>(context),
        ISagaEventHandler<SagaEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task HandleAsync(SagaEvent eventData, CancellationToken cancellationToken = default)
        {
            Context.MarkAsComplete();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        public override async Task OnStart(CreateOrderSagaData data, CancellationToken cancellationToken = default)
        {
            await Context.EventPublisher.PublishAsync(new SagaEvent(Context.Data.SagaId), cancellationToken);
        }
    }

}
