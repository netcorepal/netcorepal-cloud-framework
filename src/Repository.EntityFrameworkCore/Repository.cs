using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Repository;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{
    public abstract class RepositoryBase<TEntity, TDbContext> : IRepository<TEntity> where TEntity : Entity, IAggregateRoot where TDbContext : AppDbContextBase
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


    public abstract class RepositoryBase<TEntity, TKey, TDbContext> : RepositoryBase<TEntity, TDbContext>, IRepository<TEntity, TKey> where TEntity : Entity<TKey>, IAggregateRoot where TDbContext : AppDbContextBase where TKey : notnull
    {
        protected RepositoryBase(TDbContext context) : base(context)
        {
        }

        public virtual int DeleteById(TKey id)
        {
            return DbContext.Set<TEntity>().Where(p => p.Id.Equals(id)).ExecuteDelete();
        }

        public virtual async Task<int> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<TEntity>().Where(p => p.Id.Equals(id)).ExecuteDeleteAsync(cancellationToken);
        }

        public virtual TEntity? Get(TKey id) => DbContext.Find<TEntity>(id);

        public virtual async Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return await DbContext.FindAsync<TEntity>(new object[] { id }, cancellationToken);
        }
    }
}
