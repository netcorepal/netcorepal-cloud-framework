using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class DomainEventHandlerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_DomainEventHandlerMetadataAttribute()
    {
        // TODO: 补充具体断言
        var assembly = typeof(DomainEventHandlerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventHandlerMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventHandlerMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
    }
}
