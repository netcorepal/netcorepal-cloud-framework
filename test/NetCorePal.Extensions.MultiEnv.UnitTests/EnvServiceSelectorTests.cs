using NetCorePal.Context;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MultiEnv.UnitTests;

public class EnvServiceSelectorTests
{
    [Fact]
    public void Find_With_Env_Test()
    {
        Mock<IContextAccessor> contextAccessorMock = new();
        contextAccessorMock.Setup(p => p.GetContext(It.Is<Type>(t => t == typeof(EnvContext))))
            .Returns(new EnvContext("test"));
        Mock<IServiceDiscoveryClient> serviceDiscoveryClientMock = new();

        serviceDiscoveryClientMock.Setup(p => p.GetServiceClusters())
            .Returns(new Dictionary<string, IServiceCluster>
            {
                {
                    "s1",
                    new ServiceCluster
                    {
                        Destinations = new Dictionary<string, IDestination>
                        {
                            { "test", new Destination("s1", "d1", "s1", new Dictionary<string, string>()) }
                        }
                    }
                },
                {
                    "s1-test",
                    new ServiceCluster
                    {
                        Destinations = new Dictionary<string, IDestination>
                        {
                            { "s1", new Destination("s1-test", "d1-test", "s1-test", new Dictionary<string, string>()) }
                        }
                    }
                },
                {
                    "s2",
                    new ServiceCluster
                    {
                        Destinations = new Dictionary<string, IDestination>
                        {
                            { "s2", new Destination("s2", "d2", "s2", new Dictionary<string, string>()) }
                        }
                    }
                }
            });
        EnvServiceSelector envServiceSelector = new(contextAccessorMock.Object, serviceDiscoveryClientMock.Object);
        var destinationS1 = envServiceSelector.Find("s1");
        Assert.NotNull(destinationS1);
        Assert.Equal("s1-test", destinationS1.ServiceName);

        var destinationS2 = envServiceSelector.Find("s2");
        Assert.NotNull(destinationS2);
        Assert.Equal("s2", destinationS2.ServiceName);

        var destinationS3 = envServiceSelector.Find("s3");
        Assert.Null(destinationS3);
    }


    [Fact]
    public void Find_Without_Env_Test()
    {
        Mock<IContextAccessor> contextAccessorMock = new();
        contextAccessorMock.Setup(p => p.GetContext(It.Is<Type>(t => t == typeof(EnvContext))))
            .Returns(null as EnvContext);
        Mock<IServiceDiscoveryClient> serviceDiscoveryClientMock = new();

        serviceDiscoveryClientMock.Setup(p => p.GetServiceClusters())
            .Returns(new Dictionary<string, IServiceCluster>
            {
                {
                    "s1",
                    new ServiceCluster
                    {
                        Destinations = new Dictionary<string, IDestination>
                        {
                            { "test", new Destination("s1", "d1", "s1", new Dictionary<string, string>()) }
                        }
                    }
                },
                {
                    "s1-test",
                    new ServiceCluster
                    {
                        Destinations = new Dictionary<string, IDestination>
                        {
                            { "s1", new Destination("s1-test", "d1-test", "s1-test", new Dictionary<string, string>()) }
                        }
                    }
                },
                {
                    "s2",
                    new ServiceCluster
                    {
                        Destinations = new Dictionary<string, IDestination>
                        {
                            { "s2", new Destination("s2", "d2", "s2", new Dictionary<string, string>()) }
                        }
                    }
                }
            });
        EnvServiceSelector envServiceSelector = new(contextAccessorMock.Object, serviceDiscoveryClientMock.Object);
        var destinationS1 = envServiceSelector.Find("s1");
        Assert.NotNull(destinationS1);
        Assert.Equal("s1", destinationS1.ServiceName);

        var destinationS2 = envServiceSelector.Find("s2");
        Assert.NotNull(destinationS2);
        Assert.Equal("s2", destinationS2.ServiceName);

        var destinationS3 = envServiceSelector.Find("s3");
        Assert.Null(destinationS3);
    }
}