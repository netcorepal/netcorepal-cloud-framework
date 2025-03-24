using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql.UnitTests;

public class MockHost : IAsyncLifetime
{
    private readonly RabbitMqContainer rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();


    private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder().Build();


    public IHost? HostInstance { get; set; }

    async Task RunAsync()
    {
        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<MockDbContext>(options =>
                {
                    options.UseNpgsql(postgreSqlContainer.GetConnectionString(),
                        b => { b.MigrationsAssembly(typeof(MockDbContext).Assembly.FullName); });
                });

                services.AddCap(x =>
                {
                    x.UseEntityFramework<MockDbContext>();
                    x.UseRabbitMQ(p =>
                    {
                        p.HostName = rabbitMqContainer.Hostname;
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.Port = rabbitMqContainer.GetMappedPublicPort(5672);
                        p.VirtualHost = "/";
                    });
                });

                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssemblies(typeof(MockDbContext).Assembly)
                        .AddUnitOfWorkBehaviors());

                services.AddIntegrationEvents(typeof(MockDbContext)).UseCap<MockDbContext>(capbuilder =>
                {
                    capbuilder.RegisterServicesFromAssemblies(typeof(MockDbContext));
                    capbuilder.UsePostgreSql();
                });
            })
            .Build();
        using var scope = HostInstance!.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MockDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        HostInstance.RunAsync();
    }


    public async Task InitializeAsync()
    {
        await Task.WhenAll(rabbitMqContainer.StartAsync(), postgreSqlContainer.StartAsync());
        await RunAsync();
    }

    public async Task DisposeAsync()
    {
        if (HostInstance != null)
        {
            await HostInstance.StopAsync();
        }

        await Task.WhenAll(rabbitMqContainer.StopAsync(), postgreSqlContainer.StopAsync());
    }
}