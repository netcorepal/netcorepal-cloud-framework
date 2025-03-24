using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql
{
    public class PostgreSqlCapTransactionFactory(ICapPublisher capPublisher) : ICapTransactionFactory
    {
        public INetCorePalCapTransaction CreateCapTransaction()
        {
            return ActivatorUtilities.CreateInstance<NetCorePalPostgreSqlCapTransaction>(capPublisher.ServiceProvider);
        }
    }
}