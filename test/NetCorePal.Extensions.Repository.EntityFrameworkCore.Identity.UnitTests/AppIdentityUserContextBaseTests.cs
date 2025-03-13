using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Identity;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Identity.UnitTests;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.UnitTests;

public class AppIdentityUserContextBaseTest(DbFixture db) : IClassFixture<DbFixture>
{
    public class TestDbContext : AppIdentityUserContextBase<IdentityUser>
    {
        public TestDbContext(DbContextOptions<TestDbContext> options, IMediator mediator, IServiceProvider provider) :
            base(options, mediator, provider)
        {
        }

        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }

    public class TestPublisherTransactionHandler : IPublisherTransactionHandler
    {
        public IDbContextTransaction BeginTransaction(DbContext context)
        {
            return context.Database.BeginTransaction();
        }
    }


    public record TestEntityCreatedEvent(TestEntity Entity) : IDomainEvent;

    public class TestEntityCreatedEventHandler : IDomainEventHandler<TestEntityCreatedEvent>
    {
        public static bool Error { get; set; } = false;

        public Task Handle(TestEntityCreatedEvent notification, CancellationToken cancellationToken)
        {
            if (Error)
            {
                throw new Exception();
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }

    public class TestEntity : Entity<int>
    {
        protected TestEntity()
        {
        }

        public TestEntity(string name)
        {
            Name = name;
            this.AddDomainEvent(new TestEntityCreatedEvent(this));
        }

        public string Name { get; private set; } = string.Empty;
        public RowVersion RowVersion { get; private set; } = new RowVersion();
        public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

        public void ChangeName(string name)
        {
            Name = name;
        }
    }

    [Fact]
    public void SaveChangeTest()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("test"));

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        context.Database.EnsureCreated();

        var entity = new TestEntity("abc");

        context.Entities.Add(entity);
        context.SaveChanges();

        Assert.NotEqual(0, entity.Id);
        Assert.NotNull(entity.RowVersion);
        Assert.Equal(0, entity.RowVersion.VersionNumber);
        var updateTime = entity.UpdateTime.Value;
        entity.ChangeName("test");
        var i = context.SaveChanges();
        Assert.Equal(1, i);
        Assert.Equal(1, entity.RowVersion.VersionNumber);
        Assert.True(entity.UpdateTime.Value > updateTime);
    }

    [Fact]
    public async Task SaveChangeAsyncTest()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("test"));

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();
        var entity = new TestEntity("abc");

        context.Entities.Add(entity);
        await context.SaveChangesAsync();
        Assert.NotEqual(0, entity.Id);
        Assert.NotNull(entity.RowVersion);
        Assert.Equal(0, entity.RowVersion.VersionNumber);
        var updateTime = entity.UpdateTime.Value;
        entity.ChangeName("test");
        var i = await context.SaveChangesAsync();
        Assert.Equal(1, i);
        Assert.Equal(1, entity.RowVersion.VersionNumber);
        Assert.True(entity.UpdateTime.Value > updateTime);
    }


    [Fact]
    public async Task SaveEntitiesAsyncTest()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();
        var entity = new TestEntity("abc");

        context.Entities.Add(entity);
        await context.SaveEntitiesAsync();
        Assert.NotEqual(0, entity.Id);
        Assert.NotNull(entity.RowVersion);
        Assert.Equal(0, entity.RowVersion.VersionNumber);
        var updateTime = entity.UpdateTime.Value;
        entity.ChangeName("test");
        var i = await context.SaveChangesAsync();
        Assert.Equal(1, i);
        Assert.Equal(1, entity.RowVersion.VersionNumber);
        Assert.True(entity.UpdateTime.Value > updateTime);
    }


    [Fact]
    public async Task Entity_Should_Saved_After_CommitAsync_WithOut_PublisherTransactionHandle_Test()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        await context.BeginTransactionAsync();

        var entity = new TestEntity("abc");
        context.Entities.Add(entity);
        await context.SaveChangesAsync();
        await context.CommitAsync();

        Assert.NotEqual(0, entity.Id);
        Assert.NotNull(entity.RowVersion);
        Assert.Equal(0, entity.RowVersion.VersionNumber);

        using var scope2 = provider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity2 = await context2.Entities.FindAsync(entity.Id);
        Assert.NotNull(entity2);
    }

    [Fact]
    public async Task Entity_Should_Saved_After_CommitAsync_With_PublisherTransactionHandle_Test()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));
        services.AddScoped<IPublisherTransactionHandler, TestPublisherTransactionHandler>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        await context.BeginTransactionAsync();

        var entity = new TestEntity("abc");
        context.Entities.Add(entity);
        await context.SaveChangesAsync();
        await context.CommitAsync();

        Assert.NotEqual(0, entity.Id);
        Assert.NotNull(entity.RowVersion);
        Assert.Equal(0, entity.RowVersion.VersionNumber);

        using var scope2 = provider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity2 = await context2.Entities.FindAsync(entity.Id);
        Assert.NotNull(entity2);
    }

    [Fact]
    public async Task Entity_Should_Not_Saved_After_RollbackAsync_WithOut_PublisherTransactionHandle_Test()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        await context.BeginTransactionAsync();

        var entity = new TestEntity("abc");
        context.Entities.Add(entity);
        await context.SaveChangesAsync();
        await context.RollbackAsync();

        using var scope2 = provider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity2 = await context2.Entities.FindAsync(entity.Id);
        Assert.Null(entity2);
    }

    [Fact]
    public async Task Entity_Should_Not_Saved_After_RollbackAsync_With_PublisherTransactionHandle_Test()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));
        services.AddScoped<IPublisherTransactionHandler, TestPublisherTransactionHandler>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        await context.BeginTransactionAsync();

        var entity = new TestEntity("abc");
        context.Entities.Add(entity);
        await context.SaveChangesAsync();
        await context.RollbackAsync();

        using var scope2 = provider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity2 = await context2.Entities.FindAsync(entity.Id);
        Assert.Null(entity2);
    }

    [Fact]
    public async Task
        Entity_Should_Saved_On_SaveEntitiesAsync_After_CommitAsync_WithOut_PublisherTransactionHandle_Test()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        await context.BeginTransactionAsync();

        var entity = new TestEntity("abc");
        context.Entities.Add(entity);
        await context.SaveEntitiesAsync();
        await context.CommitAsync();

        Assert.NotEqual(0, entity.Id);
        Assert.NotNull(entity.RowVersion);
        Assert.Equal(0, entity.RowVersion.VersionNumber);

        using var scope2 = provider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity2 = await context2.Entities.FindAsync(entity.Id);
        Assert.NotNull(entity2);
    }

    [Fact]
    public async Task
        Entity_Should_Not_Saved_On_SaveEntitiesAsync_After_RollbackAsync_WithOut_PublisherTransactionHandle_Test()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        await context.BeginTransactionAsync();

        var entity = new TestEntity("abc");
        context.Entities.Add(entity);
        await context.SaveEntitiesAsync();
        await context.RollbackAsync();

        using var scope2 = provider.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity2 = await context2.Entities.FindAsync(entity.Id);
        Assert.Null(entity2);
    }


    [Fact]
    public async Task Transaction_Should_Rollback_When_Error()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(typeof(AppIdentityUserContextBaseTest).Assembly));
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o =>
            o.UseMySql(db.mySqlContainer.GetConnectionString(), MySqlServerVersion.LatestSupportedServerVersion));

        var mockTransaction = new Mock<IDbContextTransaction>();
        var rollbacked = false;
        mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Callback(() => rollbacked = true);
        services.AddScoped<IPublisherTransactionHandler>(p =>
        {
            var mock = new Mock<IPublisherTransactionHandler>();
            mock.Setup(x => x.BeginTransaction(It.IsAny<DbContext>()))
                .Returns(mockTransaction.Object);
            return mock.Object;
        });

        TestEntityCreatedEventHandler.Error = true;

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        var entity = new TestEntity("abc");
        context.Entities.Add(entity);
        await Assert.ThrowsAsync<Exception>(() => context.SaveEntitiesAsync());
        Assert.Null(context.CurrentTransaction);
        Assert.True(rollbacked);
        TestEntityCreatedEventHandler.Error = false;
    }
}