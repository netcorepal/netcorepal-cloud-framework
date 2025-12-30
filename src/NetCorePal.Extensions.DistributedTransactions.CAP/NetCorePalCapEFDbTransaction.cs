using System.Data.Common;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

#pragma warning disable S3881
public class NetCorePalCapEFDbTransaction :
#pragma warning restore S3881
    IDbContextTransaction,
    IInfrastructure<DbTransaction>
{
    private readonly INetCorePalCapTransaction _transaction;

    public INetCorePalCapTransaction CapTransaction => this._transaction;

    public NetCorePalCapEFDbTransaction(INetCorePalCapTransaction capTransaction)
    {
        this._transaction = capTransaction;
    }

    public void Dispose() => this._transaction.Dispose();

    public void Commit() => this._transaction.Commit();

    public void Rollback() => this._transaction.Rollback();

    public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return this._transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return this._transaction.RollbackAsync(cancellationToken);
    }

    public Guid TransactionId => ((IDbContextTransaction)this._transaction.DbTransaction!).TransactionId;

    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }

    public DbTransaction Instance
    {
        get => ((IDbContextTransaction)this._transaction.DbTransaction!).GetDbTransaction();
    }
}