using Consul;
using Microsoft.Extensions.Hosting;

namespace NetCorePal.Extensions.Snowflake.Consul.UnitTests;

[Collection("consul")]
public class ConsulWorkerIdGeneratorBuilderExtensionTests : IClassFixture<TestContainerFixture>
{
    private readonly ConsulContainer _consulContainer;


    public ConsulWorkerIdGeneratorBuilderExtensionTests(TestContainerFixture testContainerFixture)
    {
        _consulContainer = testContainerFixture.ConsulContainer;
    }


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
}