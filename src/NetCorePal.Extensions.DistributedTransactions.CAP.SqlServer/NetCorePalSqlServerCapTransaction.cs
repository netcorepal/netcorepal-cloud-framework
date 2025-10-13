using DotNetCore.CAP.Transport;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer;

public class NetCorePalSqlServerCapTransaction(IDispatcher dispatcher)
    : NetCorePalCapTransaction(dispatcher)
{
}