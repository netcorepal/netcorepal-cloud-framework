using Xunit;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class IntegrationEventHandlerToCommandMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_IntegrationEventHandlerToCommandMetadataAttribute()
    {
        var assembly = typeof(TestClasses.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventHandlerToCommandMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventHandlerToCommandMetadataAttribute>()
            .ToList();
        var businessAttrs = attrs.Where(a => a.HandlerType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.")).ToList();
        Assert.Equal(3, businessAttrs.Count);
        Assert.Contains(businessAttrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler" && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderCreatedIntegrationEvent" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateUserCommand"));
        Assert.Contains(businessAttrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.OrderPaidIntegrationEventHandler" && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderPaidIntegrationEvent" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ChangeOrderNameCommand"));
        Assert.Contains(businessAttrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.ExternalSystemNotificationHandler" && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.ExternalSystemNotificationEvent" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ChangeOrderNameCommand"));
    }
} 