using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Sqlite.UnitTests;

public partial class NetCorePalDataStorageDbContext(
    DbContextOptions<NetCorePalDataStorageDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator), ISqliteCapDataStorage
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NetCorePalDataStorageDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests.MockEntity> MockEntities  => Set<NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests.MockEntity>();
}
