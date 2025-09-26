using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{
    /// <summary>
    /// 事务工作单元
    /// </summary>
    public interface ITransactionUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 开启事务，该方法不会设置 CurrentTransaction 的值
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置当前工作单元的事务
        /// 此处为了支持 async 开启事务，同时使得CAP组件正常工作，所以需要手动设置 CurrentTransaction，以确保事务提交之前，CAP事件不会被发出
        /// see: https://github.com/dotnetcore/CAP/issues/1656
        /// </summary>
        IDbContextTransaction? CurrentTransaction { get; set; }

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