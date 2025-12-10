using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MySqlConnector;
using NetCorePal.Context;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.Tenant;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

[Collection("ShardingCore")]
public class ShardingTenantDbContextTests : IAsyncLifetime
{
    public ShardingTenantDbContextTests()
    {
        NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled = false;
    }

    private readonly MySqlContainer _mySqlContainer0 = new MySqlBuilder()
        .WithDatabase("sharding")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    MySqlContainer _mySqlContainer1 = new MySqlBuilder()
        .WithDatabase("sharding")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    IHost _host = null!;

    [Fact]
    public async Task ShardingTenantDbContext_ShardingTableByArea_Test()
    {
        Assert.True(NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled);
        await SendCommand(new CreateShardingTenantOrderCommand(0, "0", DateTime.Now));
        await SendCommand(new CreateShardingTenantOrderCommand(0, "1", DateTime.Now.AddMonths(-1)));
        await SendCommand(new CreateShardingTenantOrderCommand(0, "0", DateTime.Now.AddMonths(-2)));

        await Task.Delay(3000);
        await using var scope2 = _host.Services.CreateAsyncScope();
        var dbContext2 = scope2.ServiceProvider.GetRequiredService<ShardingTenantDbContext>();
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
        cmd.CommandText = "select count(1) from shardingtentorders";
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
        cmd1.CommandText = "select count(1) from shardingtentorders";
        var count1 = await cmd1.ExecuteScalarAsync();
        Assert.Equal(1L, count1);

        //CAP PublishedMessage
        var cmdpublish1 = con1.CreateCommand();
        cmdpublish1.CommandText = $"select count(1) from {NetCorePalStorageOptions.PublishedMessageTableName}";
        var countPublish1 = await cmdpublish1.ExecuteScalarAsync();
        Assert.Equal(2L, countPublish1);
    }

    private async Task SendCommand(CreateShardingTenantOrderCommand command)
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var contextAccessor = scope.ServiceProvider.GetRequiredService<IContextAccessor>();
        contextAccessor.SetContext(new TenantContext(command.Area));
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
                services.AddTenantContext().AddCapContextProcessor();
                //添加租户数据源提供程序
                services.AddSingleton<ITenantDataSourceProvider, ShardingTenantDataSourceProvider>();
                services.AddScoped(p => new Mock<ShardingTableDbContext>().Object);
                services.AddScoped(p => new Mock<ShardingDatabaseDbContext>().Object);

                services.AddRepositories(typeof(ShardingTenantOrderRepository).Assembly);
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(ShardingTenantDbContextTests).Assembly)
                        .AddTenantShardingBehavior()
                        .AddUnitOfWorkBehaviors());
                services.AddShardingDbContext<ShardingTenantDbContext>()
                    .UseNetCorePal(options =>
                    {
                        options.AllDataSourceNames = ["Db0", "Db1"];
                        options.DefaultDataSourceName = "Db0";
                    })
                    .UseRouteConfig(op =>
                    {
                        op.AddCapShardingDataSourceRoute();
                        op.AddShardingDataSourceRoute<ShardingTenantOrderVirtualDataSourceRoute>();
                    }).UseConfig(op =>
                    {
                        op.ThrowIfQueryRouteNotMatch = true;
                        op.UseShardingQuery((conStr, builder) =>
                        {
                            builder.UseMySql(conStr,
                                new MySqlServerVersion(new Version(8, 0, 34)));
                        });
                        op.UseShardingTransaction((con, builder) =>
                        {
                            builder.UseMySql(con,
                                new MySqlServerVersion(new Version(8, 0, 34)));
                        });
                        op.AddDefaultDataSource("Db0", _mySqlContainer0.GetConnectionString());
                        op.AddExtraDataSource(sp => new Dictionary<string, string>
                        {
                            { "Db1", _mySqlContainer1.GetConnectionString() }
                        });
                        op.UseShardingMigrationConfigure(b =>
                            b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>());
                    })
                    .ReplaceService<IDbContextCreator, ShardingTenantDbContextCreator>()
                    .AddShardingCore();
                services.AddCap(op =>
                {
                    op.UseNetCorePalStorage<ShardingTenantDbContext>();
                    op.UseRabbitMQ(p =>
                    {
                        p.HostName = _rabbitMqContainer.Hostname;
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.Port = _rabbitMqContainer.GetMappedPublicPort(5672);
                        p.VirtualHost = "/";
                    });
                });

                services.AddIntegrationEvents(typeof(ShardingTenantDbContext))
                    .UseCap<ShardingTenantDbContext>(capbuilder =>
                    {
                        capbuilder.AddContextIntegrationFilters();
                        capbuilder.RegisterServicesFromAssemblies(typeof(ShardingTenantDbContext));
                    });
            })
            .Build();

        await using (var scope = _host.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ShardingTenantDbContext>();
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