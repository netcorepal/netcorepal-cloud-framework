using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MySqlConnector;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

[Collection("ShardingCore")]
public class ShardingDatabaseDbContextTests : IAsyncLifetime
{
    public ShardingDatabaseDbContextTests()
    {
        NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled = false;
    }

    private readonly MySqlContainer _mySqlContainer0 = new MySqlBuilder()
        .WithDatabase("sharding")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    private readonly MySqlContainer _mySqlContainer1 = new MySqlBuilder()
        .WithDatabase("sharding")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    IHost _host = null!;

    [Fact]
    public async Task ShardingDatabaseDbContext_ShardingTableByArea_Test()
    {
        Assert.True(NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled);
        await SendCommand(new CreateShardingDatabaseOrderCommand(0, "Db0", DateTime.Now));
        await SendCommand(new CreateShardingDatabaseOrderCommand(0, "Db1", DateTime.Now.AddMonths(-1)));
        await SendCommand(new CreateShardingDatabaseOrderCommand(0, "Db0", DateTime.Now.AddMonths(-2)));

        await Task.Delay(3000);
        await using var scope2 = _host.Services.CreateAsyncScope();
        var dbContext2 = scope2.ServiceProvider.GetRequiredService<ShardingDatabaseDbContext>();
        var orders = await dbContext2.Orders.ToListAsync();
        Assert.Equal(3, orders.Count);

        var publishedMessages = await dbContext2.PublishedMessages.ToListAsync();
        Assert.Equal(6, publishedMessages.Count);
        Assert.Equal(4, publishedMessages.Count(p => p.DataSourceName == "Db0"));
        Assert.Equal(2, publishedMessages.Count(p => p.DataSourceName == "Db1"));
        foreach (var message in publishedMessages)
        {
            Assert.Equal("Succeeded", message.StatusName);
        }

        var receivedMessages = await dbContext2.ReceivedMessages.ToListAsync();
        Assert.Equal(3, receivedMessages.Count);
        foreach (var message in receivedMessages)
        {
            Assert.Equal("Succeeded", message.StatusName);
        }

        await using var con = new MySqlConnection(_mySqlContainer0.GetConnectionString());
        con.Open();
        var cmd = con.CreateCommand();
        cmd.CommandText = "select count(1) from shardingdatabaseorders";
        var count = await cmd.ExecuteScalarAsync();
        Assert.Equal(2L, count);

        //CAP PublishedMessage
        var cmdpublish = con.CreateCommand();
        cmdpublish.CommandText = $"select count(1) from {NetCorePalStorageOptions.PublishedMessageTableName}";
        var countPublish = await cmdpublish.ExecuteScalarAsync();
        Assert.Equal(4L, countPublish);

        await using var con1 = new MySqlConnection(_mySqlContainer1.GetConnectionString());
        con1.Open();
        var cmd1 = con1.CreateCommand();
        cmd1.CommandText = "select count(1) from shardingdatabaseorders";
        var count1 = await cmd1.ExecuteScalarAsync();
        Assert.Equal(1L, count1);

        //CAP PublishedMessage
        var cmdpublish1 = con1.CreateCommand();
        cmdpublish1.CommandText = $"select count(1) from {NetCorePalStorageOptions.PublishedMessageTableName}";
        var countPublish1 = await cmdpublish1.ExecuteScalarAsync();
        Assert.Equal(2L, countPublish1);

        await using var scope = _host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShardingDatabaseDbContext>();
        var list = await dbContext.Orders.ToListAsync();
        Assert.Equal(3, list.Count);
        foreach (var order in list)
        {
            Assert.Equal(1L, order.Money);
        }
    }

    private async Task SendCommand(CreateShardingDatabaseOrderCommand command)
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(command);
    }


    public async Task InitializeAsync()
    {
        await Task.WhenAll(_mySqlContainer0.StartAsync(),
            _mySqlContainer1.StartAsync(),
            _rabbitMqContainer.StartAsync());
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddScoped(_ => new Mock<ShardingTenantDbContext>().Object);
                services.AddScoped(_ => new Mock<ShardingTableDbContext>().Object);

                services.AddRepositories(typeof(ShardingDatabaseOrderRepository).Assembly);
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(ShardingDatabaseDbContextTests).Assembly)
                        .AddShardingBehavior()
                        .AddUnitOfWorkBehaviors());
                services.AddShardingDbContext<ShardingDatabaseDbContext>()
                    .UseNetCorePal(op =>
                    {
                        op.AllDataSourceNames = ["Db0", "Db1"];
                        op.DefaultDataSourceName = "Db0";
                    })
                    .UseRouteConfig(op =>
                    {
                        op.AddShardingDataSourceRoute<ShardingDatabaseOrderVirtualDataSourceRoute>();
                        op.AddCapShardingDataSourceRoute();
                    }).UseConfig(op =>
                    {
                        op.ThrowIfQueryRouteNotMatch = true;
                        op.UseShardingQuery((conStr, builder) =>
                        {
                            builder.UseMySql(conStr,
                                new MySqlServerVersion(new Version(8, 0, 34)));
                        });
                        op.UseShardingTransaction((conStr, builder) =>
                        {
                            builder.UseMySql(conStr,
                                new MySqlServerVersion(new Version(8, 0, 34)));
                        });
                        op.AddDefaultDataSource("Db0", _mySqlContainer0.GetConnectionString());
                        op.AddExtraDataSource(_ => new Dictionary<string, string>
                        {
                            { "Db1", _mySqlContainer1.GetConnectionString() }
                        });
                        op.UseShardingMigrationConfigure(b =>
                            b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>());
                    })
                    .ReplaceService<IDbContextCreator, ShardingDatabaseDbContextCreator>()
                    .AddShardingCore();
                services.AddCap(op =>
                {
                    op.UseNetCorePalStorage<ShardingDatabaseDbContext>();
                    op.UseRabbitMQ(p =>
                    {
                        p.HostName = _rabbitMqContainer.Hostname;
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.Port = _rabbitMqContainer.GetMappedPublicPort(5672);
                        p.VirtualHost = "/";
                    });
                });

                services.AddIntegrationEvents(typeof(ShardingDatabaseDbContext))
                    .UseCap<ShardingDatabaseDbContext>(capbuilder =>
                    {
                        capbuilder.RegisterServicesFromAssemblies(typeof(ShardingDatabaseDbContext));
                    });
            })
            .Build();

        await using (var scope = _host.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ShardingDatabaseDbContext>();
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(() => dbContext.Database.MigrateAsync());
        }

        _host.Services.UseAutoTryCompensateTable();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        _host.StartAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await Task.WhenAll(_mySqlContainer0.StopAsync(),
            _mySqlContainer1.StopAsync(),
            _rabbitMqContainer.StopAsync());
    }
}