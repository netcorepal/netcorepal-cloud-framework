using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCorePal.Context;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.ServiceDiscovery;

namespace NetCorePal.Extensions.MultiEnv.UnitTests;

public class EnvIntegrationEventHandlerFilterTests
{
    private readonly Mock<IServiceDiscoveryClient> _serviceDiscoveryClient;
    private readonly IContextAccessor _contextAccessor = new ContextAccessor();

    private readonly Mock<ILogger<EnvIntegrationEventHandlerFilter>> _logger =
        new Mock<ILogger<EnvIntegrationEventHandlerFilter>>();

    public EnvIntegrationEventHandlerFilterTests()
    {
        _serviceDiscoveryClient = new Mock<IServiceDiscoveryClient>();
        _serviceDiscoveryClient.Setup(p => p.GetServiceClusters()).Returns(new Dictionary<string, IServiceCluster>
        {
            {
                "test",
                new ServiceCluster
                {
                    Destinations = new Dictionary<string, IDestination>
                    {
                        { "test", new Destination("test", "d1", "test", new Dictionary<string, string>()) }
                    }
                }
            },
            {
                "test-v1",
                new ServiceCluster
                {
                    Destinations = new Dictionary<string, IDestination>
                    {
                        { "test", new Destination("test-v1", "d1-v1", "test-v1", new Dictionary<string, string>()) }
                    }
                }
            }
        });
    }


    [Fact]
    public async Task Default_Service_Next_Should_Invoked_When_Env_Empty()
    {
        // Arrange
        var envOptions = new Mock<IOptions<EnvOptions>>();
        envOptions.Setup(p => p.Value).Returns(new EnvOptions
        {
            ServiceName = "test",
            ServiceEnv = ""
        });
        
        var filter = new EnvIntegrationEventHandlerFilter(_contextAccessor, _serviceDiscoveryClient.Object,
            envOptions.Object, _logger.Object);
        var context = new IntegrationEventHandlerContext(new object(), new Dictionary<string, string?>());
        var nextValue = "";
        var next = new IntegrationEventHandlerDelegate(context =>
        {
            nextValue = "next";
            return Task.CompletedTask;
        });
        // Act
        await filter.HandleAsync(context, next);
        // Assert
        Assert.Equal("next", nextValue);
    }
    
    [Fact]
    public async Task Default_Service_Next_Should_Invoked_When_Env_Not_Exist()
    {
        _contextAccessor.SetContext(new EnvContext("v2"));
        // Arrange
        var envOptions = new Mock<IOptions<EnvOptions>>();
        envOptions.Setup(p => p.Value).Returns(new EnvOptions
        {
            ServiceName = "test",
            ServiceEnv = ""
        });
        
        var filter = new EnvIntegrationEventHandlerFilter(_contextAccessor, _serviceDiscoveryClient.Object,
            envOptions.Object, _logger.Object);
        var context = new IntegrationEventHandlerContext(new object(), new Dictionary<string, string?>());
        var nextValue = "";
        var next = new IntegrationEventHandlerDelegate(context =>
        {
            nextValue = "next";
            return Task.CompletedTask;
        });
        // Act
        await filter.HandleAsync(context, next);
        // Assert
        Assert.Equal("next", nextValue);
    }
    
    [Fact]
    public async Task Default_Service_Next_Should_Not_Invoked_When_Env_Not_Match()
    {
        _contextAccessor.SetContext(new EnvContext("v1"));
        // Arrange
        var envOptions = new Mock<IOptions<EnvOptions>>();
        envOptions.Setup(p => p.Value).Returns(new EnvOptions
        {
            ServiceName = "test",
            ServiceEnv = ""
        });
        
        var filter = new EnvIntegrationEventHandlerFilter(_contextAccessor, _serviceDiscoveryClient.Object,
            envOptions.Object, _logger.Object);
        var context = new IntegrationEventHandlerContext(new object(), new Dictionary<string, string?>());
        var nextValue = "";
        var next = new IntegrationEventHandlerDelegate(context =>
        {
            nextValue = "next";
            return Task.CompletedTask;
        });
        // Act
        await filter.HandleAsync(context, next);
        // Assert
        Assert.Equal("", nextValue);
    }
    
    [Fact]
    public async Task Env_Service_Next_Should_Invoked_When_Env_Match()
    {
        _contextAccessor.SetContext(new EnvContext("v1"));
        // Arrange
        var envOptions = new Mock<IOptions<EnvOptions>>();
        envOptions.Setup(p => p.Value).Returns(new EnvOptions
        {
            ServiceName = "test",
            ServiceEnv = "v1"
        });
        
        var filter = new EnvIntegrationEventHandlerFilter(_contextAccessor, _serviceDiscoveryClient.Object,
            envOptions.Object, _logger.Object);
        var context = new IntegrationEventHandlerContext(new object(), new Dictionary<string, string?>());
        var nextValue = "";
        var next = new IntegrationEventHandlerDelegate(context =>
        {
            nextValue = "next";
            return Task.CompletedTask;
        });
        // Act
        await filter.HandleAsync(context, next);
        // Assert
        Assert.Equal("next", nextValue);
    }
    
    
    [Fact]
    public async Task Env_Service_Next_Should_Not_Invoked_When_Env_Empty()
    {
        // Arrange
        var envOptions = new Mock<IOptions<EnvOptions>>();
        envOptions.Setup(p => p.Value).Returns(new EnvOptions
        {
            ServiceName = "test",
            ServiceEnv = "v1"
        });
        
        var filter = new EnvIntegrationEventHandlerFilter(_contextAccessor, _serviceDiscoveryClient.Object,
            envOptions.Object, _logger.Object);
        var context = new IntegrationEventHandlerContext(new object(), new Dictionary<string, string?>());
        var nextValue = "";
        var next = new IntegrationEventHandlerDelegate(context =>
        {
            nextValue = "next";
            return Task.CompletedTask;
        });
        // Act
        await filter.HandleAsync(context, next);
        // Assert
        Assert.Equal("", nextValue);
    }
    
    [Fact]
    public async Task Env_Service_Next_Should_Not_Invoked_When_Env_Exist()
    {
        _contextAccessor.SetContext(new EnvContext("v3"));
        // Arrange
        var envOptions = new Mock<IOptions<EnvOptions>>();
        envOptions.Setup(p => p.Value).Returns(new EnvOptions
        {
            ServiceName = "test",
            ServiceEnv = "v1"
        });
        
        var filter = new EnvIntegrationEventHandlerFilter(_contextAccessor, _serviceDiscoveryClient.Object,
            envOptions.Object, _logger.Object);
        var context = new IntegrationEventHandlerContext(new object(), new Dictionary<string, string?>());
        var nextValue = "";
        var next = new IntegrationEventHandlerDelegate(context =>
        {
            nextValue = "next";
            return Task.CompletedTask;
        });
        // Act
        await filter.HandleAsync(context, next);
        // Assert
        Assert.Equal("", nextValue);
    }
}