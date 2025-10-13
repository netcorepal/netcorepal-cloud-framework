using DotNetCore.CAP.Transport;
using NetCorePal.Extensions.DistributedTransactions.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql;

public class NetCorePalMySqlCapTransaction(IDispatcher dispatcher)
    : NetCorePalCapTransaction(dispatcher)
{
}