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
        // 断言与实际生成的 DomainEventMetadataAttribute 保持一致
        var expected = new[]
        {
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodDomainEvent",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntityNameChangedDomainEvent"
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var evt in expected)
        {
            Assert.Contains(attrs, a => a.EventType == evt);
        }
    }
}
