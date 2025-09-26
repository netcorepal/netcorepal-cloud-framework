using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql;

public class NetCorePalMySqlCapTransaction(IDispatcher dispatcher)
    : MySqlCapTransaction(dispatcher), INetCorePalCapTransaction
{
    public ValueTask DisposeAsync()
    {
        return ((IDbContextTransaction)DbTransaction!).DisposeAsync();
    }
}