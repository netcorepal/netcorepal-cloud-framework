using Xunit;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class DomainEventToIntegrationEventMetadataGeneratorTests
{
    // 移除 Generator_CanBeInstantiated 测试方法

    [Fact]
    public void Should_Generate_DomainEventToIntegrationEventMetadataAttribute()
    {
        var assembly = typeof(TestClasses.IntegrationEventConverters.OrderCreatedIntegrationEventConverter).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventToIntegrationEventMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventToIntegrationEventMetadataAttribute>()
            .ToList();
        var businessAttrs = attrs.Where(a => a.DomainEventType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.Order") || a.DomainEventType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.User")).ToList();
        Assert.Equal(6, businessAttrs.Count);
        Assert.Contains(businessAttrs, a => a.DomainEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderCreatedDomainEvent" && a.IntegrationEventTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderCreatedIntegrationEvent"));
        Assert.Contains(businessAttrs, a => a.DomainEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderPaidDomainEvent" && a.IntegrationEventTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderPaidIntegrationEvent"));
        Assert.Contains(businessAttrs, a => a.DomainEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderPaidDomainEvent" && a.IntegrationEventTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderDeletedIntegrationEvent"));
        Assert.Contains(businessAttrs, a => a.DomainEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserCreatedDomainEvent" && a.IntegrationEventTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.OrderCreatedIntegrationEvent"));
    }
} 