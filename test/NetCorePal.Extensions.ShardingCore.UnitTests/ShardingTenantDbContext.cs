using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DistributedTransactions.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Tenant;
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
    IShardingCore, IMySqlCapDataStorage
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
        AddDomainEvent(new ShardingTenantOrderCreatedDomainEvent(this));
    }

    public long Money { get; private set; }
    public string Area { get; private set; } = string.Empty;
    public DateTime CreationTime { get; private set; }

    public void Update(long money)
    {
        Money = money;
        AddDomainEvent(new ShardingTenantOrderMoneyUpdatedDomainEvent(this));
    }
}

public record ShardingTenantOrderCreatedDomainEvent(ShardingTenantOrder Order) : IDomainEvent;

public record ShardingTenantOrderMoneyUpdatedDomainEvent(ShardingTenantOrder Order) : IDomainEvent;

public record ShardingTenantOrderCreatedIntegrationEvent(
    ShardingTenantOrderId Id,
    long Money,
    string Area,
    DateTime CreationTime);

public record ShardingTenantOrderMoneyUpdatedIntegrationEvent(
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

public class ShardingTenantOrderMoneyUpdatedIntegrationEventConverter
    : IIntegrationEventConverter<ShardingTenantOrderMoneyUpdatedDomainEvent,
        ShardingTenantOrderMoneyUpdatedIntegrationEvent>
{
    public ShardingTenantOrderMoneyUpdatedIntegrationEvent Convert(
        ShardingTenantOrderMoneyUpdatedDomainEvent domainEvent)
    {
        return new ShardingTenantOrderMoneyUpdatedIntegrationEvent(domainEvent.Order.Id,
            domainEvent.Order.Money,
            domainEvent.Order.Area,
            domainEvent.Order.CreationTime);
    }
}

public class ShardingTenantOrderCreatedIntegrationEventHandler(IMediator mediator) :
    IIntegrationEventHandler<ShardingTenantOrderCreatedIntegrationEvent>
{
    public Task HandleAsync(ShardingTenantOrderCreatedIntegrationEvent eventData,
        CancellationToken cancellationToken = default)
    {
        return mediator.Send(new UpdateShardingTenantOrderCommand(
            eventData.Id,
            eventData.Money + 1), cancellationToken);
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

public class ShardingTenantOrderVirtualDataSourceRoute(
    IOptions<NetCorePalShardingCoreOptions> options,
    ITenantDataSourceProvider provider) :
    TenantVirtualDataSourceRoute<ShardingTenantOrder, string>(options, provider)
{
    public override void Configure(EntityMetadataDataSourceBuilder<ShardingTenantOrder> builder)
    {
        builder.ShardingProperty(p => p.Area);
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

public class ShardingTenantDataSourceProvider : ITenantDataSourceProvider
{
    public string GetDataSourceName(string tenantId)
    {
        return "Db" + tenantId;
    }
}