using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class DomainEventHandlerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_DomainEventHandlerMetadataAttribute()
    {
        var assembly = typeof(DomainEventHandlerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventHandlerMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventHandlerMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        Assert.Equal(2, attrs.Count);
        Assert.Contains(attrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEventHandler"
            && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEvent"
            && a.CommandTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEventHandlerCommand1","NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEventHandlerCommand2"}));
        Assert.Contains(attrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodDomainEventHandler"
            && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodDomainEvent"
            && a.CommandTypes.Length == 0);
    }
}
