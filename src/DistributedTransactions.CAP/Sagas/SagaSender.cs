using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public class SagaSender<TSaga, TSagaData>
        where TSaga : Saga<TSagaData>
        where TSagaData : SagaData
    {
        protected TimeSpan Timeout = TimeSpan.FromSeconds(30);

        protected ISagaContext<TSagaData> Context { get; private set; }

        protected readonly IEFCoreUnitOfWork _unitOfWork;

        readonly TSaga _saga;

        public SagaSender(ISagaContext<TSagaData> context, TSaga saga, IEFCoreUnitOfWork unitOfWork)
        {
            Context = context;
            _saga = saga;
            _unitOfWork = unitOfWork;
        }

        public virtual async Task ExecuteAsync(TSagaData sagaData, CancellationToken cancellationToken)
        {
            using (var tran = _unitOfWork.BeginTransaction())
            {
                try
                {
                    await Context.StartNewSagaAsync(sagaData, cancellationToken);
                    await Start(sagaData, cancellationToken);
                    await tran.CommitAsync(cancellationToken);
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
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
                    throw new SagaTimeoutException("WaitForComplete Timeout");
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
        public SagaSender(ISagaContext<TSagaData> context, TSaga saga, IEFCoreUnitOfWork unitOfWork) : base(context, saga, unitOfWork)
        {
        }

        public async Task<TResult?> ExecuteWithResultAsync(TSagaData sagaData, CancellationToken cancellationToken)
        {
            using (var tran = _unitOfWork.BeginTransaction())
            {
                try
                {
                    await Context.StartNewSagaAsync(sagaData, cancellationToken);
                    await Start(sagaData, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitAsync(cancellationToken);
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }

            }
            await WaitForComplete(cancellationToken);
            return Context.Data.Result;
        }


    }
}