using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public class ShardingTableDbContextTests : IAsyncLifetime
{
    MySqlContainer _mySqlContainer = new MySqlBuilder()
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
        await using var scope = _host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShardingTableDbContext>();
        //var r = await dbContext.Database.EnsureCreatedAsync();
        //Assert.True(r);

        var order1 = new ShardingTableOrder(0, "area1", DateTime.Now);
        var order2 = new ShardingTableOrder(0, "area2", DateTime.Now.AddMonths(-1));
        var order3 = new ShardingTableOrder(0, "area3", DateTime.Now.AddMonths(-2));

        await dbContext.Orders.AddAsync(order1);
        await dbContext.Orders.AddAsync(order2);
        await dbContext.Orders.AddAsync(order3);
        await dbContext.SaveChangesAsync();

        await using var scope2 = _host.Services.CreateAsyncScope();
        var dbContext2 = scope2.ServiceProvider.GetRequiredService<ShardingTableDbContext>();
        var orders = await dbContext2.Orders.ToListAsync();
        Assert.Equal(3, orders.Count);
    }


    public async Task InitializeAsync()
    {
        await Task.WhenAll(_mySqlContainer.StartAsync(), _rabbitMqContainer.StartAsync());
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(ShardingTableDbContextTests).Assembly));
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
                        op.UseShardingTransaction((conStr, builder) =>
                        {
                            builder.UseMySql(conStr,
                                new MySqlServerVersion(new Version(8, 0, 34)));
                        });
                        op.AddDefaultDataSource("ds0", _mySqlContainer.GetConnectionString());
                    })
                    .ReplaceService<IDbContextCreator, ShardingTableDbContextCreator>()
                    .AddShardingCore();
                services.AddCap(op =>
                {
                    op.UseEntityFramework<ShardingTableDbContext>();
                    op.UseRabbitMQ(p =>
                    {
                        p.HostName = _rabbitMqContainer.Hostname;
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.Port = _rabbitMqContainer.GetMappedPublicPort(5672);
                        p.VirtualHost = "/";
                    });
                });
                
                services.AddIntegrationEvents(typeof(ShardingTableDbContext)).UseCap<ShardingTableDbContext>(capbuilder =>
                {
                    capbuilder.RegisterServicesFromAssemblies(typeof(ShardingTableDbContext));
                    capbuilder.UseMySql();
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