using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB.UnitTests;

public class MongoDBNetCorePalDataStorageTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:8.0")
        .Build();
    
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    private IHost _host = null!;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
        
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(NetCorePalDataStorageTestsBase<>).Assembly)
                        .AddUnitOfWorkBehaviors());
                services.AddDbContext<NetCorePalDataStorageDbContext>(options =>
                {
                    options.UseMongoDB(_mongoContainer.GetConnectionString(), "test_cap_db");
                });
                services.AddCap(op =>
                {
                    // Use MongoDB-specific storage implementation
                    op.UseMongoDBNetCorePalStorage<NetCorePalDataStorageDbContext>();
                    op.UseRabbitMQ(p =>
                    {
                        p.HostName = _rabbitMqContainer.Hostname;
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.Port = _rabbitMqContainer.GetMappedPublicPort(5672);
                        p.VirtualHost = "/";
                    });
                });

                services.AddIntegrationEvents(typeof(NetCorePalDataStorageDbContext), typeof(NetCorePalDataStorageTestsBase<>))
                    .UseCap<NetCorePalDataStorageDbContext>(capbuilder =>
                    {
                        capbuilder.RegisterServicesFromAssemblies(typeof(NetCorePalDataStorageDbContext),
                            typeof(NetCorePalDataStorageTestsBase<>));
                    });
            })
            .Build();
            
        await using var scope = _host.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<NetCorePalDataStorageDbContext>();
        if (!await context.Database.EnsureCreatedAsync())
        {
            await context.Database.MigrateAsync();
        }
#pragma warning disable CS4014
        _host.StartAsync();
#pragma warning restore CS4014
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await _rabbitMqContainer.StopAsync();
        await _mongoContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_MongoDB()
    {
        // Test basic storage operations
        var storage = _host.Services.GetRequiredService<DotNetCore.CAP.Persistence.IDataStorage>();
        
        // Test lock operations
        Assert.True(await storage.AcquireLockAsync("test_lock", TimeSpan.FromSeconds(10), "instance1"));
        Assert.False(await storage.AcquireLockAsync("test_lock", TimeSpan.FromSeconds(10), "instance2"));
        
        await storage.ReleaseLockAsync("test_lock", "instance1");
        Assert.True(await storage.AcquireLockAsync("test_lock", TimeSpan.FromSeconds(10), "instance2"));
        
        // Test message storage
        var message = new DotNetCore.CAP.Messages.Message(new Dictionary<string, string?>
        {
            ["cap-msg-id"] = Guid.NewGuid().ToString(),
            ["cap-msg-name"] = "test.message"
        }, "test content");
        
        var stored = await storage.StoreMessageAsync("test.message", message);
        Assert.NotNull(stored);
        Assert.NotNull(stored.DbId);
    }
}
