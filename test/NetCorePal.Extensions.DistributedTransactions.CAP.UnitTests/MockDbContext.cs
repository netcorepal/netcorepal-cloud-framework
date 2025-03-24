using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

public partial class MockDbContext(
    DbContextOptions<MockDbContext> options,
    IMediator mediator,
    IServiceProvider serviceProvider) :
    AppDbContextBase(options, mediator, serviceProvider)
{
    public DbSet<MockEntity> MockEntities  => Set<MockEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MockEntity>().HasKey(mockEntity => mockEntity.Id);
        base.OnModelCreating(modelBuilder);
    }
}

public partial record MoId : IInt64StronglyTypedId;

public class MockEntity : Entity<int>
{
    public string Name { get; private set; }

    public MockEntity(string name)
    {
        Name = name;
        this.AddDomainEvent(new MockEntityCreatedDomainEvent(this));
    }
}

public record MockEntityCreatedDomainEvent(MockEntity Entity) : IDomainEvent;

public record MockEntityCreatedIntegrationEvent(int Id, string Name);

public class MockEntityCreatedDomainEventConverter : IIntegrationEventConverter<MockEntityCreatedDomainEvent,
    MockEntityCreatedIntegrationEvent>
{
    public MockEntityCreatedIntegrationEvent Convert(MockEntityCreatedDomainEvent domainEvent)
    {
        return new MockEntityCreatedIntegrationEvent(domainEvent.Entity.Id, domainEvent.Entity.Name);
    }
}

public class MockEntityCreatedIntegrationEventHandler : IIntegrationEventHandler<MockEntityCreatedIntegrationEvent>
{
    public static DateTimeOffset LastTime { get; set; } = DateTimeOffset.MinValue;

    public Task HandleAsync(MockEntityCreatedIntegrationEvent eventData,
        CancellationToken cancellationToken = default)
    {
        LastTime = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }
}