using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB.UnitTests;

public partial class NetCorePalDataStorageDbContext(
    DbContextOptions<NetCorePalDataStorageDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator), IMongoDBCapDataStorage
{
    public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();
    public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();
    public DbSet<CapLock> CapLocks => Set<CapLock>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IMongoDBCapDataStorage).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests.MockEntity> MockEntities  => Set<NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests.MockEntity>();
}
