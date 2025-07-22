using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class DomainEventMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_DomainEventMetadataAttribute()
    {
        var assembly = typeof(DomainEventMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // TestClasses 中所有领域事件类型（共9个）
        var expected = new[]
        {
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderCreatedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderPaidDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderNameChangedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderDeletedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserCreatedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserActivatedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserDeactivatedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderItemAddedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderItemQuantityUpdatedDomainEvent"
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var evt in expected)
        {
            Assert.Contains(attrs, a => a.EventType == evt);
        }
    }
}
