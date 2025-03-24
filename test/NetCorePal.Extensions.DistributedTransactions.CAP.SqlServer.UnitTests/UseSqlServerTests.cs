using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using System.Reflection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer.UnitTests
{
    public class UseSqlServerTests
    {
        [Fact]
        public void UseSqlServer_Should_Add_SqlServerCapTransactionFactory()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true);
            IConfigurationRoot configurationRoot = configurationBuilder.Build();
            services.AddLogging();
            services.AddDbContext<MockDbContext>(c => { c.UseSqlServer("localhost"); });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddCap(x =>
            {
                x.UseEntityFramework<MockDbContext>();
                x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
            });
            services.AddIntegrationEvents(typeof(UseSqlServerTests))
                .UseCap<MockDbContext>(b => b.UseSqlServer());

            var provider = services.BuildServiceProvider();


            var handler = provider.GetRequiredService<ICapTransactionFactory>();
            Assert.IsType<SqlServerCapTransactionFactory>(handler);
        }
    }
}