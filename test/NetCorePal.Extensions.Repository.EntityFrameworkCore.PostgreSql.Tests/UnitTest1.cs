using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using System.Reflection;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.PostgreSql.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true);
            IConfigurationRoot configurationRoot = configurationBuilder.Build();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddPostgreSqlTransactionHandler();
            services.AddCap(x =>
            {
                x.UseEntityFramework<ApplicationDbContext>();
                x.UseRabbitMQ(p => configurationRoot.GetSection("RabbitMQ").Bind(p));
            });


            var provider = services.BuildServiceProvider();


            var context= provider.GetRequiredService<ApplicationDbContext>();
        }
    }

    public class ApplicationDbContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : AppDbContextBase(options, mediator, provider)
    {
    }
}