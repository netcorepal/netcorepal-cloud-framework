using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql
{
    [Obsolete("此类已废弃，不再需要使用数据库特定的事务工厂。请使用通用的 NetCorePalCapTransactionFactory。This class is obsolete. Please use the generic NetCorePalCapTransactionFactory instead.")]
    public class MySqlCapTransactionFactory(ICapPublisher capPublisher) : ICapTransactionFactory
    {
        public INetCorePalCapTransaction CreateCapTransaction()
        {
            return ActivatorUtilities.CreateInstance<NetCorePalMySqlCapTransaction>(capPublisher.ServiceProvider);
        }
    }
}