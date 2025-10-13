using DotNetCore.CAP.Transport;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql;

public class NetCorePalMySqlCapTransaction(IDispatcher dispatcher)
    : NetCorePalCapTransaction(dispatcher)
{
}