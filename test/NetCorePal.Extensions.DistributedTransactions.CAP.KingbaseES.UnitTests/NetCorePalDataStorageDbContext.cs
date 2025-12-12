using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.KingbaseES.UnitTests;

public partial class NetCorePalDataStorageDbContext(
    DbContextOptions<NetCorePalDataStorageDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator), IKingbaseESCapDataStorage
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }
        //解决设置自增报错的问题
        modelBuilder.Entity<MockEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NetCorePalDataStorageDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<MockEntity> MockEntities => Set<MockEntity>();
}
