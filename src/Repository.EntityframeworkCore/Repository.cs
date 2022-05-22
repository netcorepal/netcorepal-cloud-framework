using NetCorePal.Extensions.Domain.Abstractions;
using NetCorePal.Extensions.Repository.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore
{
    public abstract class RepositoryBase<TEntity, TDbContext> : IRepository<TEntity> where TEntity : Entity, IAggregateRoot where TDbContext : EFContext
    {
        protected virtual TDbContext DbContext { get; set; }

        protected RepositoryBase(TDbContext context) => DbContext = context;
        public virtual IUnitOfWork UnitOfWork => DbContext;

        public virtual TEntity Add(TEntity entity) => DbContext.Add(entity).Entity;

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = await DbContext.AddAsync(entity, cancellationToken);
            return entry.Entity;
        }

        public virtual TEntity Update(TEntity entity) => DbContext.Update(entity).Entity;

        public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.FromResult(Update(entity));

        public virtual bool Remove(Entity entity)
        {
            DbContext.Remove(entity);
            return true;
        }

        public virtual Task<bool> RemoveAsync(Entity entity) => Task.FromResult(Remove(entity));
    }


    public abstract class RepositoryBase<TEntity, TKey, TDbContext> : RepositoryBase<TEntity, TDbContext>, IRepository<TEntity, TKey> where TEntity : Entity<TKey>, IAggregateRoot where TDbContext : EFContext where TKey : struct
    {
        protected RepositoryBase(TDbContext context) : base(context)
        {
        }

        public virtual bool Delete(TKey id)
        {
            var entity = DbContext.Find<TEntity>(id);
            if (entity is null)
            {
                return false;
            }
            DbContext.Remove(entity);
            return true;
        }

        public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await DbContext.FindAsync<TEntity>(new object[] { id }, cancellationToken);
            if (entity is null)
            {
                return false;
            }
            DbContext.Remove(entity);
            return true;
        }

        public virtual TEntity? Get(TKey id) => DbContext.Find<TEntity>(id);

        public virtual async Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default) => await DbContext.FindAsync<TEntity>(new object[] { id }, cancellationToken);
    }
}
