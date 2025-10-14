using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql;

[Obsolete("此类已废弃，不再需要使用数据库特定的事务类。请使用通用的 NetCorePalCapTransaction。This class is obsolete. Please use the generic NetCorePalCapTransaction instead.")]
public class NetCorePalMySqlCapTransaction(IDispatcher dispatcher)
    : MySqlCapTransaction(dispatcher), INetCorePalCapTransaction
{
    public ValueTask DisposeAsync()
    {
        return ((IDbContextTransaction)DbTransaction!).DisposeAsync();
    }
}