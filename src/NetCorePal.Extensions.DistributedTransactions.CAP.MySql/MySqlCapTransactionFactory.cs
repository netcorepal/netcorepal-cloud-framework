using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql
{
    public class MySqlCapTransactionFactory(ICapPublisher capPublisher) : ICapTransactionFactory
    {
        public INetCorePalCapTransaction CreateCapTransaction()
        {
            return ActivatorUtilities.CreateInstance<NetCorePalCapTransaction>(capPublisher.ServiceProvider);
        }
    }
}