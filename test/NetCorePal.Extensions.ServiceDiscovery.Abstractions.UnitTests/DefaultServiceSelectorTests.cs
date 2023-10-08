using Microsoft.Extensions.Primitives;

namespace NetCorePal.Extensions.ServiceDiscovery.Abstractions.UnitTests;

public class DefaultServiceSelectorTests
{
    [Fact]
    public void Clusters_Reload_When_ChangeToken_Changed()
    {
        CancellationTokenSource currentTokenSource = new();
        var mock = new Mock<IServiceDiscoveryProvider>();
        mock.Setup(p => p.GetReloadToken()).Returns(() =>
        {
            var cts = new CancellationTokenSource();
            var token = new CancellationChangeToken(cts.Token);
            currentTokenSource = cts;
            return token;
        });
        var list = new List<IServiceCluster>
        {
            new ServiceCluster { ClusterId = "c1" },
            new ServiceCluster { ClusterId = "c2" }
        };
        mock.Setup(p => p.Clusters).Returns(list);
        IServiceDiscoveryProvider provider = mock.Object;

        var client = new DefaultServiceDiscoveryClient(new[] { provider });
        var serviceClusters = client.GetServiceClusters();
        Assert.Equal(2, serviceClusters.Count);
        list.RemoveAt(1);
        var currentToken = client.GetReloadToken();
        Assert.False(currentToken.HasChanged);
        currentTokenSource.Cancel();
        serviceClusters = client.GetServiceClusters();
        Assert.Single(serviceClusters);
        Assert.True(currentToken.HasChanged);
    }
}