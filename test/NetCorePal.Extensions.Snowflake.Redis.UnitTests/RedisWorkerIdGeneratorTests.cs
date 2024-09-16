using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Testcontainers.Redis;
using HealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace NetCorePal.Extensions.Snowflake.Redis.UnitTests;

[Collection("redis")]
public class RedisWorkerIdGeneratorTests : IClassFixture<TestContainerFixture>
{
    private readonly RedisContainer _redisContainer;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;

    public RedisWorkerIdGeneratorTests(TestContainerFixture consulContainer)
    {
        _redisContainer = consulContainer.RedisContainer;
        _connectionMultiplexer = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
        _database = _connectionMultiplexer.GetDatabase();
    }


    [Fact]
    public void GetId_Test()
    {
        for (int i = 0; i < 32; i++)
        {
            var consulWorkerIdGenerator = CreateRedisWorkerIdGenerator();
            Assert.Equal(i, consulWorkerIdGenerator.GetId());
        }

        Assert.Throws<AggregateException>(() => CreateRedisWorkerIdGenerator());
    }


    [Fact]
    public async Task Refresh_Throw_Exception_When_Session_Locked_By_Others_Test()
    {
        HealthCheckContext healthCheckContext = new HealthCheckContext();
        var workerIdGenerator = CreateRedisWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });

        var id = workerIdGenerator.GetId();
        Assert.Equal(0, id);
        Assert.True(workerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Healthy, (await workerIdGenerator.CheckHealthAsync(healthCheckContext)).Status);

        var value = await _database.StringGetAsync(workerIdGenerator.GetWorkerIdKey());
        Assert.True(value.HasValue);

        await _database.KeyDeleteAsync(workerIdGenerator.GetWorkerIdKey());

        var consulWorkerIdGenerator2 = CreateRedisWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });
        Assert.Equal(0, consulWorkerIdGenerator2.GetId());

        await Assert.ThrowsAsync<WorkerIdConflictException>(async () => await workerIdGenerator.Refresh());
        Assert.False(workerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Unhealthy,
            (await workerIdGenerator.CheckHealthAsync(healthCheckContext)).Status);
    }


    [Fact]
    public async Task Refresh_Throw_Exception_When_Session_Locked_By_Others_And_UnhealthyStatus_Degraded_Test()
    {
        HealthCheckContext healthCheckContext = new HealthCheckContext();
        var workerIdGenerator = CreateRedisWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-degraded";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
            p.UnhealthyStatus = HealthStatus.Degraded;
        });

        var id = workerIdGenerator.GetId();
        Assert.Equal(0, id);
        Assert.True(workerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Healthy, (await workerIdGenerator.CheckHealthAsync(healthCheckContext)).Status);

        var value = await _database.StringGetAsync(workerIdGenerator.GetWorkerIdKey());
        Assert.True(value.HasValue);

        await _database.KeyDeleteAsync(workerIdGenerator.GetWorkerIdKey());

        var workerIdGenerator2 = CreateRedisWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-degraded";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });
        Assert.Equal(0, workerIdGenerator2.GetId());

        await Assert.ThrowsAsync<WorkerIdConflictException>(async () => await workerIdGenerator.Refresh());
        Assert.False(workerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Degraded,
            (await workerIdGenerator.CheckHealthAsync(healthCheckContext)).Status);
    }


    [Fact]
    public async Task Refresh_OK_When_Session_Released_And_Not_Lock_By_Others_Test()
    {
        HealthCheckContext healthCheckContext = new HealthCheckContext();
        var consulWorkerIdGenerator = CreateRedisWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-nolock";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });

        var id = consulWorkerIdGenerator.GetId();
        Assert.Equal(0, id);
        Assert.True(consulWorkerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Healthy, (await consulWorkerIdGenerator.CheckHealthAsync(healthCheckContext)).Status);

        var value = await _database.StringGetAsync(consulWorkerIdGenerator.GetWorkerIdKey());
        Assert.True(value.HasValue);

        await _database.KeyDeleteAsync(consulWorkerIdGenerator.GetWorkerIdKey());

        await consulWorkerIdGenerator.Refresh();

        var consulWorkerIdGenerator2 = CreateRedisWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-nolock";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });
        Assert.Equal(1, consulWorkerIdGenerator2.GetId());
        Assert.True(consulWorkerIdGenerator2.IsHealth);
        Assert.Equal(HealthStatus.Healthy,
            (await consulWorkerIdGenerator2.CheckHealthAsync(healthCheckContext)).Status);
    }

    [Fact]
    public void Init_Should_Fail_When_Consul_Server_Fail()
    {
        LoggerFactory loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<RedisWorkerIdGenerator>();
        var option = new RedisWorkerIdGeneratorOptions();
        var optionMock = new Mock<IOptions<RedisWorkerIdGeneratorOptions>>();
        optionMock.Setup(p => p.Value)
            .Returns(option);
        //var consulClient = new ConsulClient(p => p.Address = new Uri("http://localhost:8080"));

        //Assert.Throws<AggregateException>(() => new RedisWorkerIdGenerator(logger, optionMock.Object, consulClient));
    }

    private RedisWorkerIdGenerator CreateRedisWorkerIdGenerator(Action<RedisWorkerIdGeneratorOptions>? setup = null)
    {
        LoggerFactory loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<RedisWorkerIdGenerator>();
        var option = new RedisWorkerIdGeneratorOptions();
        setup?.Invoke(option);
        var optionMock = new Mock<IOptions<RedisWorkerIdGeneratorOptions>>();
        optionMock.Setup(p => p.Value)
            .Returns(option);
        var redis = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
        RedisWorkerIdGenerator consulWorkerIdGenerator =
            new RedisWorkerIdGenerator(logger, optionMock.Object, redis);
        return consulWorkerIdGenerator;
    }
}