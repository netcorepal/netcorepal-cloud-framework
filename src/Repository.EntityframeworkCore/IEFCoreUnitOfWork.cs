using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository
{
    public interface IEFCoreUnitOfWork : IUnitOfWork
    {
        IDbContextTransaction BeginTransaction();


        IDbContextTransaction? CurrentTransaction { get; }

        /// <summary>
        /// 提交并清除当前事务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
