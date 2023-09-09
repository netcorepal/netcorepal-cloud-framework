using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaRepository<TDbContext> : RepositoryBase<SagaEntity, Guid, TDbContext>
    where TDbContext : EFContext
{
    public SagaRepository(TDbContext dbContext) : base(dbContext)
    {
    }
}