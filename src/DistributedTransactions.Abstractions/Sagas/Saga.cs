using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public abstract class Saga
    {
    }

    public abstract class Saga<TData> : Saga where TData : SagaData, new()
    {
        protected TimeSpan Timeout = TimeSpan.FromSeconds(30);

        protected ISagaContext<TData> Context { get; private set; } = null!;

        public void SetContext(ISagaContext<TData> context)
        {
            this.Context = context;
        }

        public virtual async Task ExecuteAsync(TData sagaData, CancellationToken cancellationToken)
        {
            await Start(sagaData, cancellationToken);
            await WaitForComplete(cancellationToken);
        }

        protected abstract Task Start(TData sagaData, CancellationToken cancellationToken);


        public async Task WaitForComplete(CancellationToken cancellationToken)
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
    }

    public abstract class Saga<TData, TResult> : Saga<TData> where TData : SagaData<TResult>, new()
    {
        protected ISagaContext<TData> Context { get; private set; } = null!;

        public void SetContext(ISagaContext<TData> context)
        {
            this.Context = context;
        }

        public virtual async Task<TResult?> ExecuteAsync(TData sagaData, CancellationToken cancellationToken)
        {
            await Start(sagaData, cancellationToken);
            await WaitForComplete(cancellationToken);
            return Context.Data.Result;
        }
    }


    public class AbcSagaData : SagaData
    {
    }

    public class AbcSaga : Saga<AbcSagaData>, ISagaEventHandler<AbcEvent>

    {
        public Task HandleAsync(AbcEvent eventData, CancellationToken cancellationToken = default)
        {
            Context.MarkAsComplete();
            return Task.CompletedTask;
        }

        protected override Task Start(AbcSagaData sagaData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class AbcEvent
    {
    }
}