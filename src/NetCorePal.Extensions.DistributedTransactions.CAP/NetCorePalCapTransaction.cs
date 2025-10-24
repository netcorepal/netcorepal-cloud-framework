using System.Diagnostics;
using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public class NetCorePalCapTransaction(IDispatcher dispatcher)
    : CapTransactionBase(dispatcher), INetCorePalCapTransaction
{
    public override void Commit()
    {
        Debug.Assert(DbTransaction != null);

        switch (DbTransaction)
        {
            case IDbContextTransaction dbContextTransaction:
                dbContextTransaction.Commit();
                break;
        }

        Flush();
    }

    public override async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(DbTransaction != null);

        switch (DbTransaction)
        {
            case IDbContextTransaction dbContextTransaction:
                await dbContextTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                break;
        }

        await FlushAsync();
    }

    public override void Rollback()
    {
        Debug.Assert(DbTransaction != null);

        switch (DbTransaction)
        {
            case IDbContextTransaction dbContextTransaction:
                dbContextTransaction.Rollback();
                break;
        }
    }

    public override async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(DbTransaction != null);

        switch (DbTransaction)
        {
            case IDbContextTransaction dbContextTransaction:
                await dbContextTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    public ValueTask DisposeAsync()
    {
        return ((IDbContextTransaction)DbTransaction!).DisposeAsync();
    }
}