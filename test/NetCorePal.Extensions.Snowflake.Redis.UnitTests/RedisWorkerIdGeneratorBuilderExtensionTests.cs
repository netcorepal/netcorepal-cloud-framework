using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace NetCorePal.Extensions.Snowflake.Redis.UnitTests;

[Collection("consul")]
public class RedisWorkerIdGeneratorBuilderExtensionTests : IClassFixture<TestContainerFixture>
{
    private readonly RedisContainer _redisContainer;


    public RedisWorkerIdGeneratorBuilderExtensionTests(TestContainerFixture testContainerFixture)
    {
        _redisContainer = testContainerFixture.RedisContainer;
    }


    [Fact]
    public void AddConsulWorkerIdGeneratorTest()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddConsulWorkerIdGenerator(p => { });
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));

        var provider = services.BuildServiceProvider();

        var workIdGenerator = provider.GetService<IWorkIdGenerator>();
        IEnumerable<IHostedService> hostedServices = provider.GetServices<IHostedService>();
        var consulWorkerIdGenerator = provider.GetService<RedisWorkerIdGenerator>();
        Assert.NotNull(workIdGenerator);
        Assert.NotEmpty(hostedServices);
        Assert.NotNull(consulWorkerIdGenerator);
        Assert.Equal(workIdGenerator, consulWorkerIdGenerator);
    }
}