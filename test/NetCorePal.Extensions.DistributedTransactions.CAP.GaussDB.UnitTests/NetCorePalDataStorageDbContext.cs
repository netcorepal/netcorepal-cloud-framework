using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.GaussDB.UnitTests;

public partial class NetCorePalDataStorageDbContext(
    DbContextOptions<NetCorePalDataStorageDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator), IGaussDBCapDataStorage
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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NetCorePalDataStorageDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<MockEntity> MockEntities => Set<MockEntity>();
}
