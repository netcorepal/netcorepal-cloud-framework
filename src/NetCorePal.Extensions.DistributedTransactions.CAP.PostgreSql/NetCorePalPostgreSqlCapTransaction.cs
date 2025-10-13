using DotNetCore.CAP.Transport;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql;

public class NetCorePalPostgreSqlCapTransaction(IDispatcher dispatcher)
    : NetCorePalCapTransaction(dispatcher)
{
}