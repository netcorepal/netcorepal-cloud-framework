using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql;

public class NetCorePalPostgreSqlCapTransaction(IDispatcher dispatcher)
    : PostgreSqlCapTransaction(dispatcher), INetCorePalCapTransaction
{
    public ValueTask DisposeAsync()
    {
        return ((IDbContextTransaction)DbTransaction!).DisposeAsync();
    }
}