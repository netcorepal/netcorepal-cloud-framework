namespace NetCorePal.ServiceDiscovery.Eureka.UnitTests;


public class EurekaServiceDiscoveryProviderTests
{
    [Fact]
    public async Task GetClustersTest()
    {

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging(b => b.AddConsole());
        services.AddEurekaServiceDiscovery(p => {
            p.AppName = "test app2";
            p.ServerUrl = "http://localhost:8080/eureka/v2";
        });

        var serviceProvider = services.BuildServiceProvider();


        var provider = serviceProvider.GetService<IServiceDiscoveryProvider>();

        var clusters = provider?.Clusters;


        await Task.Delay(10000);
        serviceProvider.Dispose();
    }
}
