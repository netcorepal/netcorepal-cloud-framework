using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.ServiceDiscovery.Abstractions.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNetCorePalServiceDiscoveryClient_Should_Add_ServiceDiscoveryClient_And_ServiceSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNetCorePalServiceDiscoveryClient();

        // Assert
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IServiceDiscoveryClient>());
        Assert.NotNull(provider.GetRequiredService<IServiceSelector>());
    }
}