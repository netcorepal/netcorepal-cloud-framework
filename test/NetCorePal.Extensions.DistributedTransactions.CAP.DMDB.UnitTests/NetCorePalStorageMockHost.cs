using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
using Testcontainers.DMDB;
using Testcontainers.RabbitMq;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.DMDB.UnitTests;

public class NetCorePalStorageMockHost : IAsyncLifetime
{
    private readonly RabbitMqContainer rabbitMqContainer = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    private readonly DmdbContainer _dmDbContainer = new DmdbBuilder()
        .WithUsername("mydb")
        .WithPassword("MyPassword123")
        .WithDbaPassword("SYSDBA_abc123")
        .Build();

    public IHost? HostInstance { get; set; }

    async Task RunAsync()
    {
        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<NetCorePalDataStorageDbContext>(options =>
                {
                    options.UseDm(_dmDbContainer.GetConnectionString());
                });

                services.AddCap(x =>
                {
                    x.UseNetCorePalStorage<NetCorePalDataStorageDbContext>();
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
                    cfg.RegisterServicesFromAssemblies(typeof(MockEntity).Assembly)
                        .AddUnitOfWorkBehaviors());

                services.AddIntegrationEvents(typeof(MockEntity)).UseCap<NetCorePalDataStorageDbContext>(capbuilder =>
                {
                    capbuilder.RegisterServicesFromAssemblies(typeof(MockEntity));
                });
            })
            .Build();
        using var scope = HostInstance!.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<NetCorePalDataStorageDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
#pragma warning disable CS4014
        HostInstance.RunAsync();
#pragma warning restore CS4014
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(rabbitMqContainer.StartAsync(), _dmDbContainer.StartAsync());
        await RunAsync();
        await Task.Delay(5000);
    }

    public async Task DisposeAsync()
    {
        if (HostInstance != null)
        {
            await HostInstance.StopAsync();
        }

        await Task.WhenAll(rabbitMqContainer.StopAsync(), _dmDbContainer.StopAsync());
    }
}
