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
        var services = new ServiceCollection();

        services.AddContextCore();
        services.AddNetCorePalServiceDiscoveryClient();
        services.AddLogging();
        Action<EnvOptions> configure = p => { };

        var builder = services.AddMultiEnv(configure)
            .UseNetCorePalServiceDiscovery();

        // Assert
        Assert.NotNull(builder);
        var provider = services.BuildServiceProvider();
        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<EnvOptions>>().Value);
        Assert.Equal("EnvOptions.ServiceName is required", ex.Failures.First());
        ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IIntegrationEventHandlerFilter>());
        Assert.Equal("EnvOptions.ServiceName is required", ex.Failures.First());
    }

    [Fact]
    public void AddEnvIntegrationFilters_Should_Add_Configured_Options_Test()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCapContextProcessor();
        services.AddNetCorePalServiceDiscoveryClient();
        services.AddLogging();
        services.AddIntegrationEvents(typeof(ServiceCollectionExtensionsTests))
            .UseCap(b => b.AddContextIntegrationFilters());

        Action<EnvOptions>? configure = options =>
        {
            options.ServiceName = "test";
            options.ServiceEnv = "test";
        };

        var builder = services.AddMultiEnv(configure)
            .UseNetCorePalServiceDiscovery();
        // Assert
        Assert.NotNull(builder);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<EnvOptions>>().Value;
        Assert.NotNull(options);
        Assert.Equal("test", options.ServiceName);
        Assert.Equal("test", options.ServiceEnv);
        var filter = provider.GetServices<IIntegrationEventHandlerFilter>().ToList();
        Assert.NotNull(filter);
        Assert.Equal(2, filter.Count);
        Assert.IsType<EnvIntegrationEventHandlerFilter>(filter.Last());
    }

    [Fact]
    public void AddEnvIntegrationFilters_Should_Bind_Configuration_Test()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCapContextProcessor();
        services.AddNetCorePalServiceDiscoveryClient();
        services.AddLogging();
        services.AddIntegrationEvents(typeof(ServiceCollectionExtensionsTests))
            .UseCap(b =>
            {
                b.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensionsTests));
                b.AddContextIntegrationFilters();
            });

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "ServiceName", "test" },
            { "ServiceEnv", "test" }
        }!).Build();
        Assert.NotNull(configuration);
        var builder = services.AddMultiEnv(configuration)
            .UseNetCorePalServiceDiscovery();
        // Assert
        Assert.NotNull(builder);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<EnvOptions>>().Value;
        Assert.NotNull(options);
        Assert.Equal("test", options.ServiceName);
        Assert.Equal("test", options.ServiceEnv);
        var filter = provider.GetServices<IIntegrationEventHandlerFilter>().ToList();
        Assert.NotNull(filter);
        Assert.Equal(2, filter.Count);
        Assert.IsType<EnvIntegrationEventHandlerFilter>(filter.Last());
    }
}