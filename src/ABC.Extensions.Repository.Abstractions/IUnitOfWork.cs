using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABC.Extensions.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// 保存所有实体的变更，并协调和分发所有的领域事件
        /// </summary>
        /// <param name="cancellationToken">用于取消任务的令牌</param>
        /// <returns></returns>
        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }
}
