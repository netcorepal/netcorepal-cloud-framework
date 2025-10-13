using DotNetCore.CAP.Transport;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer;

public class NetCorePalSqlServerCapTransaction(IDispatcher dispatcher)
    : NetCorePalCapTransaction(dispatcher)
{
}