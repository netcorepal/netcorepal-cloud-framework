using DotNetCore.CAP;
using DotNetCore.CAP.SqlServer.Diagnostics;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer;

[Obsolete("此类已废弃，不再需要使用数据库特定的事务类。请使用通用的 NetCorePalCapTransaction。This class is obsolete. Please use the generic NetCorePalCapTransaction instead.")]
public class NetCorePalSqlServerCapTransaction(
    IDispatcher dispatcher,
    DiagnosticProcessorObserver diagnosticProcessorObserver)
    : SqlServerCapTransaction(dispatcher, diagnosticProcessorObserver), INetCorePalCapTransaction
{
    public ValueTask DisposeAsync()
    {
        return ((IDbContextTransaction)DbTransaction!).DisposeAsync();
    }
}