using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MySqlConnector;
using NetCorePal.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Sharding.ReadWriteConfigurations;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public class ShardingTableDbContextTests : IAsyncLifetime
{
    private readonly MySqlContainer _mySqlContainer = new MySqlBuilder()
        .WithDatabase("sharding")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    IHost _host = null!;

    [Fact]
    public async Task ShardingTableDbContext_ShardingTableByDateTime_Test()
    {
        var now = DateTime.Now;

        await SendCommand(new CreateShardingTableOrderCommand(0, "area1", now));
        await SendCommand(new CreateShardingTableOrderCommand(0, "area2", now.AddMonths(-1)));
        await SendCommand(new CreateShardingTableOrderCommand(0, "area3", now.AddMonths(-1)));
        await SendCommand(new CreateShardingTableOrderCommand(0, "area4", now.AddMonths(-2)));
        await SendCommand(new CreateShardingTableOrderCommand(0, "area5", now.AddMonths(-2)));
        await SendCommand(new CreateShardingTableOrderCommand(0, "area6", now.AddMonths(-2)));


        await using var scope2 = _host.Services.CreateAsyncScope();
        var dbContext2 = scope2.ServiceProvider.GetRequiredService<ShardingTableDbContext>();
        var orders = await dbContext2.Orders.ToListAsync();
        Assert.Equal(6, orders.Count);

        await using var con = new MySqlConnection(_mySqlContainer.GetConnectionString());
        con.Open();
        var cmd0 = con.CreateCommand();
        cmd0.CommandText = $"select count(1) from shardingtableorders_{now:yyyyMM}";
        var count0 = await cmd0.ExecuteScalarAsync();
        Assert.Equal(1L, count0);

        var cmd1 = con.CreateCommand();
        cmd1.CommandText = $"select count(1) from shardingtableorders_{now.AddMonths(-1):yyyyMM}";
        var count1 = await cmd1.ExecuteScalarAsync();
        Assert.Equal(2L, count1);

        var cmd2 = con.CreateCommand();
        cmd2.CommandText = $"select count(1) from shardingtableorders_{now.AddMonths(-2):yyyyMM}";
        var count2 = await cmd2.ExecuteScalarAsync();
        Assert.Equal(3L, count2);

        //CAP PublishedMessage
        var cmdpublish = con.CreateCommand();
        cmdpublish.CommandText = $"select count(1) from PublishedMessage";
        var countPublish = await cmdpublish.ExecuteScalarAsync();
        Assert.Equal(6L, countPublish);
    }

    private async Task SendCommand(CreateShardingTableOrderCommand command)
    {
        await using var scope = _host.Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(command);
    }


    public async Task InitializeAsync()
    {
        await Task.WhenAll(_mySqlContainer.StartAsync(), _rabbitMqContainer.StartAsync());
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddScoped(p => new Mock<ShardingTenantDbContext>().Object);
                services.AddScoped(p => new Mock<ShardingDatabaseDbContext>().Object);

                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(ShardingTableDbContextTests).Assembly)
                        .AddUnitOfWorkBehaviors());
                services.AddRepositories(typeof(ShardingTableOrderRepository).Assembly);
                services.AddShardingDbContext<ShardingTableDbContext>().UseRouteConfig(op =>
                    {
                        op.AddShardingTableRoute<OrderVirtualTableRoute>();
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
                        op.AddDefaultDataSource("ds0", _mySqlContainer.GetConnectionString());
                        op.AddReadWriteSeparation(sp => new Dictionary<string, IEnumerable<string>>
                            {
                                {
                                    "ds0",
                                    [_mySqlContainer.GetConnectionString(), _mySqlContainer.GetConnectionString()]
                                }
                            },
                            readStrategyEnum: ReadStrategyEnum.Loop,
                            defaultEnableBehavior: ReadWriteDefaultEnableBehavior.DefaultDisable,
                            defaultPriority: 10,
                            readConnStringGetStrategy: ReadConnStringGetStrategyEnum.LatestFirstTime
                        );
                    })
                    .ReplaceService<IDbContextCreator, ShardingTableDbContextCreator>()
                    .AddShardingCore();
                services.AddCap(op =>
                {
                    op.UseNetCorePalStorage<ShardingTableDbContext>();
                    op.UseRabbitMQ(p =>
                    {
                        p.HostName = _rabbitMqContainer.Hostname;
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.Port = _rabbitMqContainer.GetMappedPublicPort(5672);
                        p.VirtualHost = "/";
                    });
                });

                services.AddIntegrationEvents(typeof(ShardingTableDbContext))
                    .UseCap<ShardingTableDbContext>(capbuilder =>
                    {
                        capbuilder.RegisterServicesFromAssemblies(typeof(ShardingTableDbContext));
                    });
            })
            .Build();

        _host.Services.UseAutoTryCompensateTable();
        _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await Task.WhenAll(_mySqlContainer.StopAsync(), _rabbitMqContainer.StopAsync());
    }
}