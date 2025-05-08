using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Domain;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.VirtualRoutes.Months;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public partial class ShardingTableDbContext(
    DbContextOptions options,
    IMediator mediator) : AppDbContextBase(options, mediator), IShardingTable, IShardingDatabase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShardingTableDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }


    public DbSet<ShardingTableOrder> Orders => Set<ShardingTableOrder>();
}

public class ShardingTableDbContextCreator(IShardingProvider provider) : IDbContextCreator
{
    public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
    {
        var outDbContext = (ShardingTableDbContext)shellDbContext;
        var dbContext = new ShardingTableDbContext(shardingDbContextOptions.DbContextOptions, outDbContext.Mediator);
        if (dbContext is IShardingTableDbContext shardingTableDbContext)
        {
            shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
        }

        _ = dbContext.Model;
        return dbContext;
    }

    public DbContext GetShellDbContext(IShardingProvider shardingProvider)
    {
        return shardingProvider.GetRequiredService<ShardingTableDbContext>();
    }
}

public partial record ShardingTableOrderId : IGuidStronglyTypedId;

public class ShardingTableOrder : Entity<ShardingTableOrderId>
{
    public ShardingTableOrder(long money, string area, DateTime creationTime)
    {
        Money = money;
        Area = area;
        CreationTime = creationTime;
    }

    public long Money { get; private set; }
    public string Area { get; private set; }
    public DateTime CreationTime { get; private set; }
}

public class ShardingTableOrderEntityTypeConfiguration : IEntityTypeConfiguration<ShardingTableOrder>
{
    public void Configure(EntityTypeBuilder<ShardingTableOrder> builder)
    {
        builder.Property(p => p.Id).UseGuidVersion7ValueGenerator();
    }
}

public class OrderVirtualTableRoute : AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<ShardingTableOrder>
{
    public override DateTime GetBeginTime()
    {
        return new DateTime(2025, 1, 1);
    }

    //注意一定要配置或者采用接口+标签也是可以的
    public override void Configure(EntityMetadataTableBuilder<ShardingTableOrder> builder)
    {
        builder.ShardingProperty(o => o.CreationTime);
    }


    public override bool AutoCreateTableByTime()
    {
        return true;
    }
}