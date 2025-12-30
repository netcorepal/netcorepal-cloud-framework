using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public record NetCorePalCapDbTransaction(IDbContextTransaction Transaction, object DbContext) : IDbContextTransaction,
    IInfrastructure<DbTransaction>
{
    public Guid TransactionId => Transaction.TransactionId;

    public void Commit()
    {
        Transaction.Commit();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Transaction.CommitAsync(cancellationToken);
    }

    public void Dispose()
    {
        Transaction.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return Transaction.DisposeAsync();
    }

    public void Rollback()
    {
        Transaction.Rollback();
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Transaction.RollbackAsync(cancellationToken);
    }

    public DbTransaction Instance => Transaction.GetDbTransaction();
}