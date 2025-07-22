using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class IntegrationEventHandlerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_IntegrationEventHandlerMetadataAttribute()
    {
        var assembly = typeof(IntegrationEventHandlerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventHandlerMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventHandlerMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // TestClasses 中所有集成事件处理器及其事件类型（实际有3个）
        var expected = new[]
        {
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderCreatedIntegrationEvent"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderPaidIntegrationEvent"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.ExternalSystemNotificationHandler", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.ExternalSystemNotificationEvent")
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var (handler, evt) in expected)
        {
            Assert.Contains(attrs, a => a.HandlerType == handler && a.EventType == evt);
        }
    }
}
