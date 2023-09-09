using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public class SagaSender<TSaga, TSagaData>
        where TSaga : Saga<TSagaData>
        where TSagaData : SagaData
    {
        protected TimeSpan Timeout = TimeSpan.FromSeconds(30);

        protected ISagaContext<TSagaData> Context { get; set; } = null!;

        readonly TSaga _saga;

        public SagaSender(ISagaContext<TSagaData> context, TSaga saga)
        {
            Context = context;
            _saga = saga;
        }

        public void SetContext(ISagaContext<TSagaData> context)
        {
            this.Context = context;
        }

        public virtual async Task ExecuteAsync(TSagaData sagaData, CancellationToken cancellationToken)
        {
            await Context.StartNewSagaAsync(sagaData, cancellationToken);
            await Start(sagaData, cancellationToken);
            await WaitForComplete(cancellationToken);
        }

        protected Task Start(TSagaData sagaData, CancellationToken cancellationToken)
        {
            return _saga.OnStart(sagaData, cancellationToken);
        }


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
    }

    public class SagaSender<TSaga, TSagaData, TResult> : SagaSender<TSaga, TSagaData>
        where TSaga : Saga<TSagaData, TResult>
        where TSagaData : SagaData<TResult>
    {
        public SagaSender(ISagaContext<TSagaData> context, TSaga saga) : base(context, saga)
        {
        }

        public async Task<TResult?> ExecuteWithResultAsync(TSagaData sagaData, CancellationToken cancellationToken)
        {
            await Context.StartNewSagaAsync(sagaData, cancellationToken);
            await Start(sagaData, cancellationToken);
            await WaitForComplete(cancellationToken);
            return Context.Data.Result;
        }
    }



}