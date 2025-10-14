using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer
{
    [Obsolete("此类已废弃，不再需要使用数据库特定的事务工厂。请使用通用的 NetCorePalCapTransactionFactory。This class is obsolete. Please use the generic NetCorePalCapTransactionFactory instead.")]
    public class SqlServerCapTransactionFactory(ICapPublisher capPublisher) : ICapTransactionFactory
    {
        public INetCorePalCapTransaction CreateCapTransaction()
        {
            return ActivatorUtilities.CreateInstance<NetCorePalSqlServerCapTransaction>(capPublisher.ServiceProvider);
        }
    }
}