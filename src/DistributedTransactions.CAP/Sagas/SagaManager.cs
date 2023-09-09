using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas
{
    public class SagaManager : ISagaManager
    {
        private readonly IServiceProvider _serviceProvider;

        public SagaManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public Task SendAsync<TSaga, TSagaData>(TSagaData sagaData, CancellationToken cancellationToken = default)
            where TSaga : Saga<TSagaData>
            where TSagaData : SagaData
        {
            var sender = _serviceProvider.GetRequiredService<SagaSender<TSaga, TSagaData>>();

            return sender.ExecuteAsync(sagaData, cancellationToken);
        }

        public Task<TResult?> SendAsync<TSaga, TSagaData, TResult>(TSagaData sagaData,
            CancellationToken cancellationToken = default)
            where TSaga : Saga<TSagaData, TResult>
            where TSagaData : SagaData<TResult>
        {
            var sender = _serviceProvider.GetRequiredService<SagaSender<TSaga, TSagaData, TResult>>();
            return sender.ExecuteWithResultAsync(sagaData, cancellationToken);
        }
    }
}