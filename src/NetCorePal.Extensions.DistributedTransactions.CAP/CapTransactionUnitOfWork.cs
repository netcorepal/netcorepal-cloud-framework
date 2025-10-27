using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

#pragma warning disable S3881
public class CapTransactionUnitOfWork(
#pragma warning restore S3881
    ITransactionUnitOfWork transactionUnitOfWork,
    ICapPublisher capPublisher,
    ICapTransactionFactory transactionFactory) : ITransactionUnitOfWork
{
    /// <summary>
    /// 存储当前的DbContext
    /// </summary>
    private static readonly AsyncLocal<object> Current = new AsyncLocal<object>();

    /// <summary>
    /// 获取当前DbContext
    /// </summary>
    internal static object? CurrentDbContext => Current.Value;


    public void Dispose()
    {
        transactionUnitOfWork.Dispose();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return transactionUnitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        return transactionUnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return transactionUnitOfWork.BeginTransactionAsync(cancellationToken);
    }

    public IDbContextTransaction? CurrentTransaction
    {
        get => transactionUnitOfWork.CurrentTransaction;
        set
        {
            if (value != null)
            {
                var capTransaction = transactionFactory.CreateCapTransaction();
                capTransaction.DbTransaction = value;
                capTransaction.AutoCommit = false;
                capPublisher.Transaction = capTransaction;
                if (transactionUnitOfWork is DbContext)
                {
                    Current.Value = transactionUnitOfWork;
                }

                transactionUnitOfWork.CurrentTransaction = new NetCorePalCapEFDbTransaction(capTransaction);
            }
        }
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return transactionUnitOfWork.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return transactionUnitOfWork.RollbackAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        Current.Value = null!;
        return transactionUnitOfWork.DisposeAsync();
    }
}