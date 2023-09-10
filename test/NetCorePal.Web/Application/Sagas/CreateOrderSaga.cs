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


    public class SagaEvent : ISagaEvent
    {
        public SagaEvent(Guid sagaId)
        {
            SagaId = sagaId;
        }

        public Guid SagaId { get; }
    }



    public class CreateOrderSaga : Saga<CreateOrderSagaData, long>,
        ISagaEventHandler<SagaEvent>
    {
        public CreateOrderSaga(ISagaContext<CreateOrderSagaData> context) : base(context)
        {
        }

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



    public class CreateOrderSagaAsyncSubscriber : ICapSubscribe
    {
        readonly IEFCoreUnitOfWork _unitOfWork;
        readonly CreateOrderSaga _handler;

        public CreateOrderSagaAsyncSubscriber(IEFCoreUnitOfWork unitOfWork, CreateOrderSaga handler)
        {
            _unitOfWork = unitOfWork;
            _handler = handler;
        }

        [CapSubscribe("SagaEvent")]
        public async Task ProcessAsync(NetCorePal.Web.Application.Sagas.SagaEvent message, CancellationToken cancellationToken)
        {
            using (var transaction = _unitOfWork.BeginTransaction())
            {
                try
                {
                    await _handler.Context.InitAsync(message.SagaId, cancellationToken);
                    await _handler.HandleAsync(message, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }
    }
}
