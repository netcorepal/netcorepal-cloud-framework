using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using System.Reflection;
using NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.PostgreSql.Tests
{
    public class UsePostgreSqlTests
    {
        [Fact]
        public void UsePostgreSql_Should_Add_CapPostgreSqlPublisherTransactionHandler()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true);
            IConfigurationRoot configurationRoot = configurationBuilder.Build();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddCap(x =>
            {
                x.UseEntityFramework<ApplicationDbContext>();
                x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
            });
            services.AddIntegrationEvents(typeof(UsePostgreSqlTests)).UseCap(b => b.UsePostgreSql());

            var provider = services.BuildServiceProvider();


            var handler = provider.GetRequiredService<IPublisherTransactionHandler>();
            Assert.IsType<CapPostgreSqlPublisherTransactionHandler>(handler);
            
            var context= provider.GetRequiredService<ApplicationDbContext>();
        }
    }

    public class ApplicationDbContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : AppDbContextBase(options, mediator, provider)
    {
    }
}