using System.Threading;
using System.Threading.Tasks;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.Repository
{
    /// <summary>
    /// 仓储接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public interface IRepository<TEntity> where TEntity : Entity, IAggregateRoot
    {
        /// <summary>
        /// 获取工作单元对象
        /// </summary>
        IUnitOfWork UnitOfWork { get; }
        /// <summary>
        /// 添加一个实体到仓储
        /// </summary>
        /// <param name="entity">要添加的实体对象</param>
        /// <returns></returns>
        TEntity Add(TEntity entity);
        /// <summary>
        /// 添加实体到仓储
        /// </summary>
        /// <param name="entity">要添加的实体对象</param>
        /// <param name="cancellationToken">取消操作token</param>
        /// <returns></returns>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">要更新的实体对象</param>
        /// <returns></returns>
        TEntity Update(TEntity entity);
        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">要更新的实体对象</param>
        /// <param name="cancellationToken">取消操作token</param>
        /// <returns></returns>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">要删除的实体对象</param>
        /// <returns></returns>
        bool Remove(Entity entity);
        /// <summary>
        /// 要删除的实体对象
        /// </summary>
        /// <param name="entity">要删除的实体对象</param>
        /// <returns></returns>
        Task<bool> RemoveAsync(Entity entity);
    }

    /// <summary>
    /// 仓储接口
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    public interface IRepository<TEntity, TKey> : IRepository<TEntity> where TEntity : Entity<TKey>, IAggregateRoot
    {
        /// <summary>
        /// 根据主键删除实体
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        int DeleteById(TKey id);
        /// <summary>
        /// 根据主键删除实体
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="cancellationToken">取消操作token</param>
        /// <returns></returns>
        Task<int> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);
        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns></returns>
        TEntity? Get(TKey id);
        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="cancellationToken">取消操作token</param>
        /// <returns></returns>
        Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
