using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using System.Reflection;
using NetCorePal.Extensions.DistributedTransactions.CAP.UnitTests;
namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer.UnitTests
{
    [Obsolete("此测试类测试已废弃的功能。This test class tests obsolete functionality.")]
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

        [Fact]
        public void UseSqlServer_Should_Failed_When_UseNetCorePalStorage_First()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true);
            IConfigurationRoot configurationRoot = configurationBuilder.Build();
            services.AddLogging();
            services.AddDbContext<NetCorePalDataStorageDbContext>(c => { c.UseSqlServer("localhost"); });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddCap(x =>
            {
                x.UseNetCorePalStorage<NetCorePalDataStorageDbContext>();
                x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
            });

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                services.AddIntegrationEvents(typeof(UseSqlServerTests))
                    .UseCap<NetCorePalDataStorageDbContext>(b => b.UseSqlServer());
            });

            Assert.Equal(R.RepeatAddition, ex.Message);
        }

        [Fact]
        public void UseNetCorePalStorage_Should_Failed_When_UseSqlServer_First()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true);
            IConfigurationRoot configurationRoot = configurationBuilder.Build();
            services.AddLogging();
            services.AddDbContext<NetCorePalDataStorageDbContext>(c => { c.UseSqlServer("localhost"); });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

            services.AddIntegrationEvents(typeof(UseSqlServerTests))
                .UseCap<NetCorePalDataStorageDbContext>(b => b.UseSqlServer());

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
}