using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MySqlConnector;
using NetCorePal.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.ShardingCore.UnitTests;

public class ShardingTenantDbContextTests : IAsyncLifetime
{
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
        await SendCommand(new CreateShardingTenantOrderCommand(0, "Db0", DateTime.Now));
        await SendCommand(new CreateShardingTenantOrderCommand(0, "Db1", DateTime.Now.AddMonths(-1)));
        await SendCommand(new CreateShardingTenantOrderCommand(0, "Db0", DateTime.Now.AddMonths(-2)));

        await using var scope2 = _host.Services.CreateAsyncScope();
        var dbContext2 = scope2.ServiceProvider.GetRequiredService<ShardingTenantDbContext>();
        var orders = await dbContext2.Orders.ToListAsync();
        Assert.Equal(3, orders.Count);

        await using var con = new MySqlConnection(_mySqlContainer0.GetConnectionString());
        con.Open();
        var cmd = con.CreateCommand();
        cmd.CommandText = "select count(1) from shardingtentorders";
        var count = await cmd.ExecuteScalarAsync();
        Assert.Equal(2L, count);

        await using var con1 = new MySqlConnection(_mySqlContainer1.GetConnectionString());
        con1.Open();
        var cmd1 = con1.CreateCommand();
        cmd1.CommandText = "select count(1) from shardingtentorders";
        var count1 = await cmd1.ExecuteScalarAsync();
        Assert.Equal(1L, count1);
    }

    private async Task SendCommand(CreateShardingTenantOrderCommand command)
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
                services.AddScoped(p => new Mock<ShardingTableDbContext>().Object);
                services.AddScoped(p => new Mock<ShardingDatabaseDbContext>().Object);

                services.AddRepositories(typeof(ShardingTenantOrderRepository).Assembly);
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(typeof(ShardingTenantDbContextTests).Assembly)
                        .AddUnitOfWorkBehaviors());
                services.AddShardingDbContext<ShardingTenantDbContext>().UseRouteConfig(op =>
                    {
                        op.AddShardingDataSourceRoute<ShardingTenantOrderVirtualDataSourceRoute>();
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
                        op.AddExtraDataSource(sp => new Dictionary<string, string>
                        {
                            { "Db1", _mySqlContainer1.GetConnectionString() }
                        });
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
                        capbuilder.RegisterServicesFromAssemblies(typeof(ShardingTenantDbContext));
                    });
            })
            .Build();

        _host.Services.UseAutoTryCompensateTable();
        _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await Task.WhenAll(_mySqlContainer0.StopAsync(),
            _mySqlContainer1.StopAsync(),
            _rabbitMqContainer.StopAsync());
    }
}