using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Testcontainers.MySql;
using Testcontainers.Redis;
using Testcontainers.RabbitMq;
using Testcontainers.PostgreSql;

namespace NetCorePal.Web.UnitTests
{
    public class MyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly RedisContainer redisContainer =
            new RedisBuilder().Build();

        private readonly RabbitMqContainer rabbitMqContainer = new RabbitMqBuilder()
            .WithUsername("guest").WithPassword("guest").Build();

        // private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
        //     .WithUsername("postgres").WithPassword("123456")
        //     .WithDatabase("demo").Build();
        
        private readonly MySqlContainer mySqlContainer = new MySqlBuilder()
            .WithUsername("root").WithPassword("123456")
            .WithEnvironment("TZ", "Asia/Shanghai")
            .WithDatabase("demo").Build(); 


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //builder.UseSetting("ConnectionStrings:PostgreSQL", postgreSqlContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:MySql",
                mySqlContainer.GetConnectionString().Replace("demo", $"demo"));
            builder.UseSetting("ConnectionStrings:Redis", redisContainer.GetConnectionString());
            builder.UseSetting("RabbitMQ:HostName", rabbitMqContainer.Hostname);
            builder.UseSetting("RabbitMQ:UserName", "guest");
            builder.UseSetting("RabbitMQ:Password", "guest");
            builder.UseSetting("RabbitMQ:VirtualHost", "/");
            builder.UseSetting("RabbitMQ:Port", rabbitMqContainer.GetMappedPublicPort(5672).ToString());
            builder.UseEnvironment("Development");
            base.ConfigureWebHost(builder);
        }

        public Task InitializeAsync()
        {
            return Task.WhenAll(redisContainer.StartAsync(),
                rabbitMqContainer.StartAsync(),
                mySqlContainer.StartAsync());
        }

        public new Task DisposeAsync()
        {
            return Task.WhenAll(redisContainer.StopAsync(),
                rabbitMqContainer.StopAsync(),
                mySqlContainer.StopAsync());
        }
    }
}