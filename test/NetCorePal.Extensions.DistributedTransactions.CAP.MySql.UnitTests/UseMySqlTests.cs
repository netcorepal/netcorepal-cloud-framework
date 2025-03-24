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
}