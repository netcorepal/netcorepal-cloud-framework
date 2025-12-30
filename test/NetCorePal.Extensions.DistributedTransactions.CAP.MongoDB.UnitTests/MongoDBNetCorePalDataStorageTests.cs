using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB.UnitTests;

public partial class MongoDBNetCorePalDataStorageTests : NetCorePalDataStorageTestsBase<NetCorePalDataStorageDbContext>
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithReplicaSet("rs0")
        .WithUsername("admin")
        .WithPassword("guest")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    public override async Task InitializeAsync()
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

                services.AddIntegrationEvents(typeof(NetCorePalDataStorageDbContext),
                        typeof(NetCorePalDataStorageTestsBase<>))
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

    public override async Task DisposeAsync()
    {
        await _host.StopAsync();
        await _rabbitMqContainer.StopAsync();
        await _mongoContainer.DisposeAsync();
    }

    protected override void ConfigDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMongoDB(_mongoContainer.GetConnectionString(), "test_cap_db");
    }

    [Fact]
    public async Task Test_NetCorePalDataStorage_Use_MongoDB()
    {
        await base.Storage_Tests();
    }
}