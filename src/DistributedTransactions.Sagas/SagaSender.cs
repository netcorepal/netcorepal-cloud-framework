using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public abstract class SagaSender<TData>
        where TData : SagaData
    {
        protected TimeSpan Timeout = TimeSpan.FromSeconds(30);

        protected ISagaContext<TData> Context { get; set; } = null!;

        public void SetContext(ISagaContext<TData> context)
        {
            this.Context = context;
        }

        public virtual async Task ExecuteAsync(TData sagaData, CancellationToken cancellationToken)
        {
            await Context.StartNewSagaAsync(sagaData, cancellationToken);
            await Start(sagaData, cancellationToken);
            await WaitForComplete(cancellationToken);
        }

        protected abstract Task Start(TData sagaData, CancellationToken cancellationToken);


        protected async Task WaitForComplete(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Context.RefreshAsync(cancellationToken);
                if (Context.IsTimeout())
                {
                    throw new Exception("timeout");
                }

                if (Context.IsComplete())
                {
                    return;
                }

                await Task.Delay(1000);
            }
        }

        public Task Handle(SagaStartEvent<TData> notification, CancellationToken cancellationToken)
        {
            return Start(notification.SagaData, cancellationToken);
        }
    }

    public abstract class SagaSender<TData, TResult> : SagaSender<TData>
        where TData : SagaData<TResult>
    {
        public async Task<TResult?> ExecuteWithResultAsync(TData sagaData, CancellationToken cancellationToken)
        {
            await Context.StartNewSagaAsync(sagaData, cancellationToken);
            //await Start(sagaData, cancellationToken);
            await WaitForComplete(cancellationToken);
            return Context.Data.Result;
        }
    }


    public class AbcSagaData : SagaData
    {
        public string OrderId { get; set; } = null!;
    }

    public class AbcSaga : Saga<AbcSagaData>, ISagaEventHandler<AbcEvent>
    {
        public AbcSaga(ISagaContext<AbcSagaData> context) : base(context)
        {
        }

        public override Task OnStart(AbcSagaData data, CancellationToken cancellationToken = default)
        {
            return Context.EventPublisher.PublishAsync(new AbcEvent { SagaId = Context.Data.SagaId },
                cancellationToken);
        }

        public Task HandleAsync(AbcEvent eventData, CancellationToken cancellationToken = default)
        {
            Context.MarkAsComplete();
            return Task.CompletedTask;
        }
    }

    public class AbcEvent : ISagaEvent
    {
        public Guid SagaId { get; set; }
    }
}