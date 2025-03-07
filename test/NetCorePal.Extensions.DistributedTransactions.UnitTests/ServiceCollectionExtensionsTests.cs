using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.DistributedTransactions.UnitTests.Assembly1;
using NetCorePal.Extensions.DistributedTransactions.UnitTests.Assembly2;

namespace NetCorePal.Extensions.DistributedTransactions.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddIntegrationEventServices_Should_Throw_When_2_Handler_GroupName_Not_Set()
    {
        IServiceCollection services = new ServiceCollection();
        Assert.Throws<Exception>(() => services.AddIntegrationEventServices(
            typeof(MockIntegrationEvent1)));
    }
    
    
    [Fact]
    public void AddIntegrationEventServices_Should_Throw_When_2_Handler_GroupName_Same()
    {
        IServiceCollection services = new ServiceCollection();

        Assert.Throws<Exception>(() => services.AddIntegrationEventServices(
            typeof(MockIntegrationEvent2)));
    }

    [Fact]
    public void AddIntegrationEventServices_Should_Not_Throw_When_Handler_GroupName_Not_Same()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddIntegrationEventServices(typeof(MockIntegrationEvent));
    }
}

public class MockIntegrationEvent
{
}

public class MockIntegrationEventHandler : IIntegrationEventHandler<MockIntegrationEvent>
{
    public Task HandleAsync(MockIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

[IntegrationEventConsumer(nameof(MockIntegrationEvent), "abc")]
public class MockIntegrationEventHandler2 : IIntegrationEventHandler<MockIntegrationEvent>
{
    public Task HandleAsync(MockIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}