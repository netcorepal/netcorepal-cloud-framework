using DotNetCore.CAP.Transport;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql;

public class NetCorePalPostgreSqlCapTransaction(IDispatcher dispatcher)
    : NetCorePalCapTransaction(dispatcher)
{
}