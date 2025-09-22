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
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;
using ShardingCore.Sharding.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public partial class ShardingDatabaseDbContext(
    DbContextOptions<ShardingDatabaseDbContext> options,
    IMediator mediator) : AppDbContextBase(options, mediator),
    IShardingCore, IMySqlCapDataStorage
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShardingDatabaseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<ShardingDatabaseOrder> Orders => Set<ShardingDatabaseOrder>();
}

public class ShardingDatabaseDbContextCreator(
#pragma warning disable CS9113 // 参数未读。
    IShardingProvider provider) : IDbContextCreator
#pragma warning restore CS9113 // 参数未读。
{
    public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
    {
        var outDbContext = (ShardingDatabaseDbContext)shellDbContext;
        var dbContext = new ShardingDatabaseDbContext(
            (DbContextOptions<ShardingDatabaseDbContext>)shardingDbContextOptions.DbContextOptions,
            outDbContext.Mediator);
        if (dbContext is IShardingTableDbContext ShardingDatabaseDbContext)
        {
            ShardingDatabaseDbContext.RouteTail = shardingDbContextOptions.RouteTail;
        }

        _ = dbContext.Model;
        return dbContext;
    }

    public DbContext GetShellDbContext(IShardingProvider shardingProvider)
    {
        return shardingProvider.GetRequiredService<ShardingDatabaseDbContext>();
    }
}

public partial record ShardingDatabaseOrderId : IGuidStronglyTypedId;

public class ShardingDatabaseOrder : Entity<ShardingDatabaseOrderId>, IAggregateRoot
{
    protected ShardingDatabaseOrder()
    {
    }

    public ShardingDatabaseOrder(long money, string area, DateTime creationTime)
    {
        Money = money;
        Area = area;
        CreationTime = creationTime;
        AddDomainEvent(new ShardingDatabaseOrderCreatedDomainEvent(this));
    }

    public long Money { get; private set; }
    public string Area { get; private set; } = string.Empty;
    public DateTime CreationTime { get; private set; }

    public void Update(long money)
    {
        Money = money;
        AddDomainEvent(new ShardingDatabaseOrderMoneyUpdatedDomainEvent(this));
    }
}

public record ShardingDatabaseOrderCreatedDomainEvent(ShardingDatabaseOrder Order) : IDomainEvent;

public record ShardingDatabaseOrderMoneyUpdatedDomainEvent(ShardingDatabaseOrder Order) : IDomainEvent;

public record ShardingDatabaseOrderCreatedIntegrationEvent(
    ShardingDatabaseOrderId Id,
    long Money,
    string Area,
    DateTime CreationTime);

public record ShardingDatabaseOrderMoneyUpdatedIntegrationEvent(
    ShardingDatabaseOrderId Id,
    long Money,
    string Area,
    DateTime CreationTime);

public class ShardingDatabaseOrderCreatedIntegrationEventConverter
    : IIntegrationEventConverter<ShardingDatabaseOrderCreatedDomainEvent, ShardingDatabaseOrderCreatedIntegrationEvent>
{
    public ShardingDatabaseOrderCreatedIntegrationEvent Convert(ShardingDatabaseOrderCreatedDomainEvent domainEvent)
    {
        return new ShardingDatabaseOrderCreatedIntegrationEvent(domainEvent.Order.Id,
            domainEvent.Order.Money,
            domainEvent.Order.Area,
            domainEvent.Order.CreationTime);
    }
}

public class ShardingDatabaseOrderMoneyUpdatedIntegrationEventConverter
    : IIntegrationEventConverter<ShardingDatabaseOrderMoneyUpdatedDomainEvent,
        ShardingDatabaseOrderMoneyUpdatedIntegrationEvent>
{
    public ShardingDatabaseOrderMoneyUpdatedIntegrationEvent Convert(
        ShardingDatabaseOrderMoneyUpdatedDomainEvent domainEvent)
    {
        return new ShardingDatabaseOrderMoneyUpdatedIntegrationEvent(domainEvent.Order.Id,
            domainEvent.Order.Money,
            domainEvent.Order.Area,
            domainEvent.Order.CreationTime);
    }
}

public class ShardingDatabaseOrderCreatedIntegrationEventHandler(IMediator mediator) :
    IIntegrationEventHandler<ShardingDatabaseOrderCreatedIntegrationEvent>
{
    public Task HandleAsync(ShardingDatabaseOrderCreatedIntegrationEvent eventData,
        CancellationToken cancellationToken = default)
    {
        return mediator.Send(new UpdateShardingDatabaseOrderCommand(
            eventData.Id,
            eventData.Money + 1), cancellationToken);
    }
}

public class ShardingDatabaseOrderEntityTypeConfiguration : IEntityTypeConfiguration<ShardingDatabaseOrder>
{
    public void Configure(EntityTypeBuilder<ShardingDatabaseOrder> builder)
    {
        builder.ToTable("shardingdatabaseorders");
        builder.Property(p => p.Id).UseGuidVersion7ValueGenerator();
    }
}

public class ShardingDatabaseOrderVirtualDataSourceRoute(IOptions<NetCorePalShardingCoreOptions> options)
    : NetCorePalVirtualDataSourceRoute<ShardingDatabaseOrder,
        string>(options)
{
    public override void Configure(EntityMetadataDataSourceBuilder<ShardingDatabaseOrder> builder)
    {
        builder.ShardingProperty(o => o.Area);
    }

    protected override string GetDataSourceName(object? shardingKey)
    {
        return shardingKey == null ? string.Empty : shardingKey.ToString()!;
    }
}

public class ShardingDatabaseOrderRepository(ShardingDatabaseDbContext dbContext)
    : RepositoryBase<ShardingDatabaseOrder, ShardingDatabaseOrderId, ShardingDatabaseDbContext>(dbContext)
{
}

public record CreateShardingDatabaseOrderCommand(long Money, string Area, DateTime CreationTime)
    : ICommand;

public class CreateShardingDatabaseOrderCommandHandler(ShardingDatabaseOrderRepository repository)
    : ICommandHandler<CreateShardingDatabaseOrderCommand>
{
    public async Task Handle(CreateShardingDatabaseOrderCommand request, CancellationToken cancellationToken)
    {
        await repository.AddAsync(new ShardingDatabaseOrder(request.Money, request.Area, request.CreationTime),
            cancellationToken);
    }
}

public record UpdateShardingDatabaseOrderCommand(ShardingDatabaseOrderId Id, long Money)
    : ICommand;

public class UpdateShardingDatabaseOrderCommandHandler(ShardingDatabaseOrderRepository repository)
    : ICommandHandler<UpdateShardingDatabaseOrderCommand>
{
    public async Task Handle(UpdateShardingDatabaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetAsync(request.Id, cancellationToken);
        if (order is null)
        {
            throw new Exception("订单不存在");
        }

        order.Update(request.Money);
    }
}