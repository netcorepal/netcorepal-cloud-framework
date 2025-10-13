using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql
{
    public class PostgreSqlCapTransactionFactory(ICapPublisher capPublisher) : ICapTransactionFactory
    {
        public INetCorePalCapTransaction CreateCapTransaction()
        {
            return ActivatorUtilities.CreateInstance<NetCorePalCapTransaction>(capPublisher.ServiceProvider);
        }
    }
}