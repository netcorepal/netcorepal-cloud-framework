using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.MultiEnv.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEnvIntegrationFilters_Should_Throw_If_ServiceName_Is_Empty()
    {
        // Arrange
        var builder = new Mock<IIntegrationEventServicesBuilder>();
        var services = new ServiceCollection();
        services.AddContextCore();
        services.AddNetCorePalServiceDiscoveryClient();
        services.AddLogging();
        builder.SetupGet(x => x.Services).Returns(services);
        Action<EnvOptions> configure = p => { };

        // Act
        var result = builder.Object.AddEnvIntegrationFilters(configure);
        // Assert
        Assert.NotNull(result);
        var provider = services.BuildServiceProvider();
        var ex = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<EnvOptions>>().Value);
        Assert.Equal("EnvOptions.ServiceName is required", ex.Failures.First());
        ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IIntegrationEventHandlerFilter>());
        Assert.Equal("EnvOptions.ServiceName is required", ex.Failures.First());
    }

    [Fact]
    public void AddEnvIntegrationFilters_Should_Add_Configured_Options_Test()
    {
        // Arrange
        var builder = new Mock<IIntegrationEventServicesBuilder>();
        var services = new ServiceCollection();
        services.AddContextCore();
        services.AddNetCorePalServiceDiscoveryClient();
        services.AddLogging();
        builder.SetupGet(x => x.Services).Returns(services);
        Action<EnvOptions>? configure = options =>
        {
            options.ServiceName = "test";
            options.ServiceEnv = "test";
        };

        // Act
        var result = builder.Object.AddEnvIntegrationFilters(configure);
        // Assert
        Assert.NotNull(result);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<EnvOptions>>().Value;
        Assert.NotNull(options);
        Assert.Equal("test", options.ServiceName);
        Assert.Equal("test", options.ServiceEnv);
        var filter = provider.GetRequiredService<IIntegrationEventHandlerFilter>();
        Assert.NotNull(filter);
    }
    
    [Fact]
    public void AddEnvIntegrationFilters_Should_Bind_Configuration_Test()
    {
        // Arrange
        var builder = new Mock<IIntegrationEventServicesBuilder>();
        var services = new ServiceCollection();
        services.AddContextCore();
        services.AddNetCorePalServiceDiscoveryClient();
        services.AddLogging();
        builder.SetupGet(x => x.Services).Returns(services);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            {"ServiceName", "test"},
            {"ServiceEnv", "test"}
        }!).Build();

        // Act
        var result = builder.Object.AddEnvIntegrationFilters(configuration);
        // Assert
        Assert.NotNull(result);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<EnvOptions>>().Value;
        Assert.NotNull(options);
        Assert.Equal("test", options.ServiceName);
        Assert.Equal("test", options.ServiceEnv);
        var filter = provider.GetRequiredService<IIntegrationEventHandlerFilter>();
        Assert.NotNull(filter);
    }
}