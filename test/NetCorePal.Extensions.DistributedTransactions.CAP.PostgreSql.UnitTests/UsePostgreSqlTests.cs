using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using System.Reflection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql.UnitTests;

public class UsePostgreSqlTests
{
    [Fact]
    public void UsePostgreSql_Should_Add_PostgreSqlCapTransactionFactory()
    {
        IServiceCollection services = new ServiceCollection();
        ConfigurationBuilder configurationBuilder = new();
        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        IConfigurationRoot configurationRoot = configurationBuilder.Build();
        services.AddLogging();
        services.AddDbContext<MockDbContext>(c => { c.UseNpgsql("localhost"); });
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        services.AddCap(x =>
        {
            x.UseEntityFramework<MockDbContext>();
            x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
        });
        services.AddIntegrationEvents(typeof(UsePostgreSqlTests))
            .UseCap<MockDbContext>(b => b.UsePostgreSql());

        var provider = services.BuildServiceProvider();


        var handler = provider.GetRequiredService<ICapTransactionFactory>();
        Assert.IsType<PostgreSqlCapTransactionFactory>(handler);
    }
}