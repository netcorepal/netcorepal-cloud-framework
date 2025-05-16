using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
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
    IMediator mediator) : AppDbContextBase(options, mediator),
    IShardingCore, IMySqlCapDataStorage
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

public class ShardingTableDbContextCreator
    : IDbContextCreator
{
    public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
    {
        var outDbContext = (ShardingTableDbContext)shellDbContext;
        var dbContext = new ShardingTableDbContext(
            (DbContextOptions<ShardingTableDbContext>)shardingDbContextOptions.DbContextOptions, outDbContext.Mediator);
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

public class ShardingTableOrder : Entity<ShardingTableOrderId>, IAggregateRoot
{
    protected ShardingTableOrder()
    {
    }

    public ShardingTableOrder(long money, string area, DateTime creationTime)
    {
        Money = money;
        Area = area;
        CreationTime = creationTime;
        AddDomainEvent(new ShardingTableOrderCreatedDomainEvent(this));
    }

    public long Money { get; private set; }
    public string Area { get; private set; } = string.Empty;
    public DateTime CreationTime { get; private set; }

    public void Update(long money)
    {
        Money = money;
    }
}

public record ShardingTableOrderCreatedDomainEvent(ShardingTableOrder Order) : IDomainEvent;

public record ShardingTableOrderCreatedIntegrationEvent(
    ShardingTableOrderId Id,
    long Money,
    string Area,
    DateTime CreationTime);

public class ShardingTableOrderCreatedIntegrationEventConverter()
    : IIntegrationEventConverter<ShardingTableOrderCreatedDomainEvent, ShardingTableOrderCreatedIntegrationEvent>
{
    public ShardingTableOrderCreatedIntegrationEvent Convert(ShardingTableOrderCreatedDomainEvent domainEvent)
    {
        return new ShardingTableOrderCreatedIntegrationEvent(domainEvent.Order.Id,
            domainEvent.Order.Money,
            domainEvent.Order.Area,
            domainEvent.Order.CreationTime);
    }
}

public class ShardingTableOrderCreatedIntegrationEventHandler(IMediator mediator)
    : IIntegrationEventHandler<ShardingTableOrderCreatedIntegrationEvent>
{
    public Task HandleAsync(ShardingTableOrderCreatedIntegrationEvent eventData,
        CancellationToken cancellationToken = default)
    {
        return mediator.Send(new UpdateShardingTableOrderCommand(eventData.Id, eventData.Money + 1), cancellationToken);
    }
}

public class ShardingTableOrderEntityTypeConfiguration : IEntityTypeConfiguration<ShardingTableOrder>
{
    public void Configure(EntityTypeBuilder<ShardingTableOrder> builder)
    {
        builder.ToTable("shardingtableorders");
        builder.Property(p => p.Id).UseGuidVersion7ValueGenerator();
    }
}

public class OrderVirtualTableRoute : AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<ShardingTableOrder>
{
    public override DateTime GetBeginTime()
    {
        return DateTime.Now.AddMonths(-3);
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

public class ShardingTableOrderRepository(ShardingTableDbContext dbContext)
    : RepositoryBase<ShardingTableOrder, ShardingTableOrderId, ShardingTableDbContext>(dbContext)
{
}

public record CreateShardingTableOrderCommand(long Money, string Area, DateTime CreationTime)
    : ICommand;

public class CreateShardingTableOrderCommandHandler(ShardingTableOrderRepository repository)
    : ICommandHandler<CreateShardingTableOrderCommand>
{
    public async Task Handle(CreateShardingTableOrderCommand request, CancellationToken cancellationToken)
    {
        await repository.AddAsync(new ShardingTableOrder(request.Money, request.Area, request.CreationTime),
            cancellationToken);
    }
}

public record UpdateShardingTableOrderCommand(ShardingTableOrderId Id, long Money)
    : ICommand;

public class UpdateShardingTableOrderCommandHandler(ShardingTableOrderRepository repository)
    : ICommandHandler<UpdateShardingTableOrderCommand>
{
    public async Task Handle(UpdateShardingTableOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetAsync(request.Id, cancellationToken);
        if (order is null)
        {
            throw new Exception("订单不存在");
        }

        order.Update(request.Money);
    }
}