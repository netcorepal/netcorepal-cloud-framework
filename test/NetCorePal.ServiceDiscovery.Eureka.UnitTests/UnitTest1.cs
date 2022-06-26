namespace NetCorePal.ServiceDiscovery.Eureka.UnitTests;

public class EurekaServiceDiscoveryProviderTests
{
    [Fact]
    public void GetClustersTest()
    {
        var options = Options.Create<EurekaProviderOption>(new EurekaProviderOption {



        });
        
        var provider = new EurekaServiceDiscoveryProvider(options);

        var clusters = provider.Clusters;
    }
}
