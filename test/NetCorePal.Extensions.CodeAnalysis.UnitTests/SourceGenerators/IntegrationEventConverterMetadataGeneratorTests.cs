using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class IntegrationEventConverterMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_IntegrationEventConverterMetadataAttribute()
    {
        var assembly = typeof(IntegrationEventConverterMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventConverterMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventConverterMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        Assert.Equal(3, attrs.Count);
        Assert.Contains(attrs, a => a.DomainEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEvent" && a.IntegrationEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedIntegrationEvent");
        Assert.Contains(attrs, a => a.DomainEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodDomainEvent" && a.IntegrationEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEvent");
        Assert.Contains(attrs, a => a.DomainEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodDomainEvent" && a.IntegrationEventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodIntegrationEvent2");
    }
}
