using Consul;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Testcontainers.Consul;
using HealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

namespace NetCorePal.Extensions.Snowflake.Consul.UnitTests;

[Collection("consul")]
public class ConsulWorkerIdGeneratorTests : IClassFixture<TestContainerFixture>
{
    private readonly ConsulContainer _consulContainer;
    private readonly IConsulClient _consulClient;

    public ConsulWorkerIdGeneratorTests(TestContainerFixture consulContainer)
    {
        _consulContainer = consulContainer.ConsulContainer;
        _consulClient = new ConsulClient(p => p.Address = new Uri(_consulContainer.GetBaseAddress()));
    }


    [Fact]
    public void GetId_Test()
    {
        for (int i = 0; i < 32; i++)
        {
            var consulWorkerIdGenerator = CreateConsulWorkerIdGenerator();
            Assert.Equal(i, consulWorkerIdGenerator.GetId());
        }

        Assert.Throws<AggregateException>(() => CreateConsulWorkerIdGenerator());
    }


    [Fact]
    public async Task Refresh_Throw_Exception_When_Session_Locked_By_Others_Test()
    {
        HealthCheckContext healthCheckContext = new HealthCheckContext();
        var consulWorkerIdGenerator = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });

        var id = consulWorkerIdGenerator.GetId();
        Assert.Equal(0, id);
        Assert.True(consulWorkerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Healthy, consulWorkerIdGenerator.CheckHealthAsync(healthCheckContext).Result.Status);

        var releaseResult = await _consulClient.KV.Release(new KVPair(consulWorkerIdGenerator.GetWorkerIdKey())
        {
            Session = consulWorkerIdGenerator.CurrentSessionId
        });
        Assert.True(releaseResult.Response);

        var destoryResult = await _consulClient.Session.Destroy(consulWorkerIdGenerator.CurrentSessionId);

        Assert.True(destoryResult.Response);

        var deleteResult = await _consulClient.KV.Delete(consulWorkerIdGenerator.GetWorkerIdKey());
        Assert.True(deleteResult.Response);

        var consulWorkerIdGenerator2 = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });
        Assert.Equal(0, consulWorkerIdGenerator2.GetId());

        await Assert.ThrowsAsync<WorkerIdConflictException>(async () => await consulWorkerIdGenerator.Refresh());
        Assert.False(consulWorkerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Unhealthy,
            consulWorkerIdGenerator.CheckHealthAsync(healthCheckContext).Result.Status);
    }


    [Fact]
    public async Task Refresh_Throw_Exception_When_Session_Locked_By_Others_And_UnhealthyStatus_Degraded_Test()
    {
        HealthCheckContext healthCheckContext = new HealthCheckContext();
        var consulWorkerIdGenerator = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-degraded";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
            p.UnhealthyStatus = HealthStatus.Degraded;
        });

        var id = consulWorkerIdGenerator.GetId();
        Assert.Equal(0, id);
        Assert.True(consulWorkerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Healthy, consulWorkerIdGenerator.CheckHealthAsync(healthCheckContext).Result.Status);

        var releaseResult = await _consulClient.KV.Release(new KVPair(consulWorkerIdGenerator.GetWorkerIdKey())
        {
            Session = consulWorkerIdGenerator.CurrentSessionId
        });
        Assert.True(releaseResult.Response);

        var destoryResult = await _consulClient.Session.Destroy(consulWorkerIdGenerator.CurrentSessionId);

        Assert.True(destoryResult.Response);

        var deleteResult = await _consulClient.KV.Delete(consulWorkerIdGenerator.GetWorkerIdKey());
        Assert.True(deleteResult.Response);

        var consulWorkerIdGenerator2 = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-degraded";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });
        Assert.Equal(0, consulWorkerIdGenerator2.GetId());

        await Assert.ThrowsAsync<WorkerIdConflictException>(async () => await consulWorkerIdGenerator.Refresh());
        Assert.False(consulWorkerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Degraded,
            consulWorkerIdGenerator.CheckHealthAsync(healthCheckContext).Result.Status);
    }


    [Fact]
    public async Task Refresh_OK_When_Session_Released_And_Not_Lock_By_Others_Test()
    {
        HealthCheckContext healthCheckContext = new HealthCheckContext();
        var consulWorkerIdGenerator = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-nolock";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });

        var id = consulWorkerIdGenerator.GetId();
        Assert.Equal(0, id);
        Assert.True(consulWorkerIdGenerator.IsHealth);
        Assert.Equal(HealthStatus.Healthy, consulWorkerIdGenerator.CheckHealthAsync(healthCheckContext).Result.Status);

        var releaseResult = await _consulClient.KV.Release(new KVPair(consulWorkerIdGenerator.GetWorkerIdKey())
        {
            Session = consulWorkerIdGenerator.CurrentSessionId
        });
        Assert.True(releaseResult.Response);

        var destoryResult = await _consulClient.Session.Destroy(consulWorkerIdGenerator.CurrentSessionId);

        Assert.True(destoryResult.Response);

        var deleteResult = await _consulClient.KV.Delete(consulWorkerIdGenerator.GetWorkerIdKey());
        Assert.True(deleteResult.Response);

        await consulWorkerIdGenerator.Refresh();

        var consulWorkerIdGenerator2 = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app-nolock";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });
        Assert.Equal(1, consulWorkerIdGenerator2.GetId());
        Assert.True(consulWorkerIdGenerator2.IsHealth);
        Assert.Equal(HealthStatus.Healthy, consulWorkerIdGenerator2.CheckHealthAsync(healthCheckContext).Result.Status);
    }

    [Fact]
    public void Init_Should_Fail_When_Consul_Server_Fail()
    {
        LoggerFactory loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<ConsulWorkerIdGenerator>();
        var option = new ConsulWorkerIdGeneratorOptions();
        var optionMock = new Mock<IOptions<ConsulWorkerIdGeneratorOptions>>();
        optionMock.Setup(p => p.Value)
            .Returns(option);
        var consulClient = new ConsulClient(p => p.Address = new Uri("http://localhost:8080"));

        Assert.Throws<AggregateException>(() => new ConsulWorkerIdGenerator(logger, optionMock.Object, consulClient));
    }

    private ConsulWorkerIdGenerator CreateConsulWorkerIdGenerator(Action<ConsulWorkerIdGeneratorOptions>? setup = null)
    {
        LoggerFactory loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<ConsulWorkerIdGenerator>();
        var option = new ConsulWorkerIdGeneratorOptions();
        setup?.Invoke(option);
        var optionMock = new Mock<IOptions<ConsulWorkerIdGeneratorOptions>>();
        optionMock.Setup(p => p.Value)
            .Returns(option);
        var consulClient = new ConsulClient(p => p.Address = new Uri(_consulContainer.GetBaseAddress()));
        ConsulWorkerIdGenerator consulWorkerIdGenerator =
            new ConsulWorkerIdGenerator(logger, optionMock.Object, consulClient);
        return consulWorkerIdGenerator;
    }

    [Fact]
    public async Task RenewTest()
    {
        Assert.ThrowsAsync<SessionExpiredException>(() => _consulClient.Session.Renew("aadsf234234234"));
    }
}