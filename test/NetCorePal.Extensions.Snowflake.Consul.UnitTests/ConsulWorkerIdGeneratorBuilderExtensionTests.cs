using Consul;
using Microsoft.Extensions.Hosting;

namespace NetCorePal.Extensions.Snowflake.Consul.UnitTests;

public class ConsulWorkerIdGeneratorBuilderExtensionTests : IAsyncLifetime
{
    private readonly ConsulContainer _consulContainer = new ConsulBuilder().Build();

    [Fact]
    public void AddConsulWorkerIdGeneratorTest()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddConsulWorkerIdGenerator(p => { });
        services.AddSingleton<IConsulClient>(new ConsulClient(p =>
        {
            p.Address = new Uri(_consulContainer.GetConnectionString());
        }));

        var provider = services.BuildServiceProvider();

        var workIdGenerator = provider.GetService<IWorkIdGenerator>();
        IEnumerable<IHostedService> hostedServices = provider.GetServices<IHostedService>();
        var consulWorkerIdGenerator = provider.GetService<ConsulWorkerIdGenerator>();
        Assert.NotNull(workIdGenerator);
        Assert.NotEmpty(hostedServices);
        Assert.NotNull(consulWorkerIdGenerator);
        Assert.Equal(workIdGenerator, consulWorkerIdGenerator);
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