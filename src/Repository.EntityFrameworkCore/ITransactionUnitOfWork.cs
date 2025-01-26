using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository
{
    public interface ITransactionUnitOfWork : IUnitOfWork
    {
        ValueTask<IDbContextTransaction> BeginTransactionAsync();


        IDbContextTransaction? CurrentTransaction { get; }

        /// <summary>
        /// 提交并清除当前事务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 回滚并清除当前事务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}