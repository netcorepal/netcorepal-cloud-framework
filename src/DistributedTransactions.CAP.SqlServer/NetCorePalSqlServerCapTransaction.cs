using DotNetCore.CAP;
using DotNetCore.CAP.SqlServer.Diagnostics;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer;

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