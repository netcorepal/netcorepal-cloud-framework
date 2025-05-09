using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public partial class ShardingTenantDbContext(
    DbContextOptions<ShardingTenantDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator), 
    IShardingTable, IShardingDatabase, ICapDataStorage
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShardingTenantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<ShardingTenantOrder> Orders => Set<ShardingTenantOrder>();
}

public class ShardingTenantDbContextCreator(
    IShardingProvider provider) : IDbContextCreator
{
    public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
    {
        var outDbContext = (ShardingTenantDbContext)shellDbContext;
        var dbContext = new ShardingTenantDbContext(
            (DbContextOptions<ShardingTenantDbContext>)shardingDbContextOptions.DbContextOptions,
            outDbContext.Mediator);
        if (dbContext is IShardingTableDbContext shardingTableDbContext)
        {
            shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
        }

        _ = dbContext.Model;
        return dbContext;
    }

    public DbContext GetShellDbContext(IShardingProvider shardingProvider)
    {
        return shardingProvider.GetRequiredService<ShardingTenantDbContext>();
    }
}

public partial record ShardingTenantOrderId : IGuidStronglyTypedId;

public class ShardingTenantOrder : Entity<ShardingTenantOrderId>, IAggregateRoot
{
    protected ShardingTenantOrder()
    {
    }

    public ShardingTenantOrder(long money, string area, DateTime creationTime)
    {
        Money = money;
        Area = area;
        CreationTime = creationTime;
    }

    public long Money { get; private set; }
    public string Area { get; private set; } = string.Empty;
    public DateTime CreationTime { get; private set; }
    
    public void Update(long money)
    {
        Money = money;
    }
}

public record ShardingTenantOrderCreatedDomainEvent(ShardingTenantOrder Order) : IDomainEvent;

public record ShardingTenantOrderCreatedIntegrationEvent(
    ShardingTenantOrderId Id,
    long Money,
    string Area,
    DateTime CreationTime);

public class ShardingTenantOrderCreatedIntegrationEventConverter
    : IIntegrationEventConverter<ShardingTenantOrderCreatedDomainEvent, ShardingTenantOrderCreatedIntegrationEvent>
{
    public ShardingTenantOrderCreatedIntegrationEvent Convert(ShardingTenantOrderCreatedDomainEvent domainEvent)
    {
        return new ShardingTenantOrderCreatedIntegrationEvent(domainEvent.Order.Id,
            domainEvent.Order.Money,
            domainEvent.Order.Area,
            domainEvent.Order.CreationTime);
    }
}

public class ShardingTenantOrderEntityTypeConfiguration : IEntityTypeConfiguration<ShardingTenantOrder>
{
    public void Configure(EntityTypeBuilder<ShardingTenantOrder> builder)
    {
        builder.ToTable("shardingtentorders");
        builder.Property(p => p.Id).UseGuidVersion7ValueGenerator();
    }
}

public class ShardingTenantOrderVirtualDataSourceRoute : AbstractShardingOperatorVirtualDataSourceRoute<ShardingTenantOrder, string>
{
    private readonly List<string> _dataSources = new List<string>()
    {
        "Db0", "Db1"
    };

    string ConvertToShardingKey(object shardingKey)
    {
        return shardingKey?.ToString() ?? string.Empty;
    }

    //我们设置区域就是数据库
    public override string ShardingKeyToDataSourceName(object shardingKey)
    {
        return ConvertToShardingKey(shardingKey);
    }

    public override List<string> GetAllDataSourceNames()
    {
        return _dataSources;
    }

    public override bool AddDataSourceName(string dataSourceName)
    {
        if (_dataSources.Any(o => o == dataSourceName))
            return false;
        _dataSources.Add(dataSourceName);
        return true;
    }

    public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var t = ShardingKeyToDataSourceName(shardingKey);
        switch (shardingOperator)
        {
            case ShardingOperatorEnum.Equal: return tail => tail == t;
            default:
            {
                return tail => true;
            }
        }
    }

    public override void Configure(EntityMetadataDataSourceBuilder<ShardingTenantOrder> builder)
    {
        builder.ShardingProperty(o => o.Area);
    }
}

public class ShardingTenantOrderRepository(ShardingTenantDbContext dbContext)
    : RepositoryBase<ShardingTenantOrder, ShardingTenantOrderId, ShardingTenantDbContext>(dbContext)
{
}

public record CreateShardingTenantOrderCommand(long Money, string Area, DateTime CreationTime)
    : ICommand;

public class CreateShardingTenantOrderCommandHandler(ShardingTenantOrderRepository repository)
    : ICommandHandler<CreateShardingTenantOrderCommand>
{
    public async Task Handle(CreateShardingTenantOrderCommand request, CancellationToken cancellationToken)
    {
        await repository.AddAsync(new ShardingTenantOrder(request.Money, request.Area, request.CreationTime),
            cancellationToken);
    }
}

public record UpdateShardingTenantOrderCommand(ShardingTenantOrderId Id, long Money)
    : ICommand;

public class UpdateShardingTenantOrderCommandHandler(ShardingTenantOrderRepository repository)
    : ICommandHandler<UpdateShardingTenantOrderCommand>
{
    public async Task Handle(UpdateShardingTenantOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetAsync(request.Id, cancellationToken);
        if (order is null)
        {
            throw new Exception("订单不存在");
        }

        order.Update(request.Money);
    }
}