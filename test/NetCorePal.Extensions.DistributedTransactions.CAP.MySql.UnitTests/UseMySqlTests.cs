using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using System.Reflection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql.UnitTests;

public class UseMySqlTests
{
    [Fact]
    public void UseMySql_Should_Add_MySqlCapTransactionFactory()
    {
        IServiceCollection services = new ServiceCollection();
        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        IConfigurationRoot configurationRoot = configurationBuilder.Build();
        services.AddLogging();
        services.AddDbContext<MockDbContext>(c =>
        {
            c.UseMySql("Server=any;User ID=any;Password=any;Database=any", new MySqlServerVersion("8.0.34"));
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        services.AddCap(x =>
        {
            x.UseEntityFramework<MockDbContext>();
            x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
        });
        services.AddIntegrationEvents(typeof(UseMySqlTests))
            .UseCap<MockDbContext>(b => b.UseMySql());

        var provider = services.BuildServiceProvider();


        var handler = provider.GetRequiredService<ICapTransactionFactory>();
        Assert.IsType<MySqlCapTransactionFactory>(handler);
    }

    [Fact]
    public void UseMySql_Should_Failed_When_UseNetCorePalStorage_First()
    {
        IServiceCollection services = new ServiceCollection();
        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        IConfigurationRoot configurationRoot = configurationBuilder.Build();
        services.AddLogging();
        services.AddDbContext<NetCorePalDataStorageDbContext>(c =>
        {
            c.UseMySql("Server=any;User ID=any;Password=any;Database=any", new MySqlServerVersion("8.0.34"));
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        services.AddCap(x =>
        {
            x.UseNetCorePalStorage<NetCorePalDataStorageDbContext>();
            x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddIntegrationEvents(typeof(UseMySqlTests))
                .UseCap<NetCorePalDataStorageDbContext>(b => b.UseMySql());
        });

        Assert.Equal(R.RepeatAddition, ex.Message);
    }

    [Fact]
    public void UseNetCorePalStorage_Should_Failed_When_UseMySql_First()
    {
        IServiceCollection services = new ServiceCollection();
        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        IConfigurationRoot configurationRoot = configurationBuilder.Build();
        services.AddLogging();
        services.AddDbContext<NetCorePalDataStorageDbContext>(c =>
        {
            c.UseMySql("Server=any;User ID=any;Password=any;Database=any", new MySqlServerVersion("8.0.34"));
        });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services.AddIntegrationEvents(typeof(UseMySqlTests))
            .UseCap<NetCorePalDataStorageDbContext>(b => b.UseMySql());

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddCap(x =>
            {
                x.UseNetCorePalStorage<NetCorePalDataStorageDbContext>();
                x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
            });
        });

        Assert.Equal(NetCorePal.Extensions.DistributedTransactions.CAP.R.RepeatAddition, ex.Message);
    }
}