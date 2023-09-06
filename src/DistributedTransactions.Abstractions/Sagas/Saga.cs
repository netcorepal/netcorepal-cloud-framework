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


        public virtual async Task InitAsync(TData sagaData, CancellationToken cancellationToken)
        {
            await Start(sagaData, cancellationToken);
            await WaitComplate(cancellationToken);
        }


        public abstract Task Start(TData sagaData, CancellationToken cancellationToken);


        public async Task WaitComplate(CancellationToken cancellationToken)
        {
            DateTime timeOut = DateTime.Now.Add(Timeout);
            while (!Context.Data.IsComplate)
            {
                if (DateTime.Now > timeOut)
                {
                    throw new Exception("timeout");
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

        public virtual async Task<TResult?> InitAsync(TData sagaData, CancellationToken cancellationToken)
        {
            await Start(sagaData, cancellationToken);
            await WaitComplate(cancellationToken);
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

        public override Task Start(AbcSagaData sagaData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class AbcEvent
    { }
}
