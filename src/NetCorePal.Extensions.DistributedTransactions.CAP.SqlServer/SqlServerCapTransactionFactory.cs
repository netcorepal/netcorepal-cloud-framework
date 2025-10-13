using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer
{
    public class SqlServerCapTransactionFactory(ICapPublisher capPublisher) : ICapTransactionFactory
    {
        public INetCorePalCapTransaction CreateCapTransaction()
        {
            return ActivatorUtilities.CreateInstance<NetCorePalCapTransaction>(capPublisher.ServiceProvider);
        }
    }
}