using Consul;

namespace NetCorePal.Extensions.Snowflake.Consul.UnitTests;

public class ConsulWorkerIdGeneratorTests : IAsyncLifetime
{
    private readonly ConsulContainer _consulContainer = new ConsulBuilder().Build();

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

    //[Fact]
    public async Task Session_Timeout_When_No_Refresh_Session_Test()
    {
        var consulWorkerIdGenerator = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "timeout-app";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });

        var id = consulWorkerIdGenerator.GetId();
        Assert.Equal(0, id);
        await Task.Delay(20000);
        await Assert.ThrowsAsync<SessionExpiredException>(() => consulWorkerIdGenerator.Refresh());
    }

    //[Fact]
    public async Task Session_Timeout_When_Refresh_Session_Test()
    {
        var consulWorkerIdGenerator = CreateConsulWorkerIdGenerator(p =>
        {
            p.AppName = "no-timeout-app";
            p.SessionTtlSeconds = 10;
            p.SessionRefreshIntervalSeconds = 5;
        });

        var id = consulWorkerIdGenerator.GetId();
        Assert.Equal(0, id);
        await Task.Delay(5000);
        await consulWorkerIdGenerator.Refresh();
        await Task.Delay(5000);
        await consulWorkerIdGenerator.Refresh();
        await Task.Delay(5000);
        await consulWorkerIdGenerator.Refresh();
        await Task.Delay(5000);
        await consulWorkerIdGenerator.Refresh();
        await Task.Delay(5000);
        await consulWorkerIdGenerator.Refresh();
        await Task.Delay(5000);
        await consulWorkerIdGenerator.Refresh();
    }

    //[Fact]
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
        var consulClient = new ConsulClient(p => p.Address = new Uri(_consulContainer.GetConnectionString()));
        ConsulWorkerIdGenerator consulWorkerIdGenerator =
            new ConsulWorkerIdGenerator(logger, optionMock.Object, consulClient);
        return consulWorkerIdGenerator;
    }

    public Task InitializeAsync()
    {
        return _consulContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _consulContainer.StopAsync();
    }
}