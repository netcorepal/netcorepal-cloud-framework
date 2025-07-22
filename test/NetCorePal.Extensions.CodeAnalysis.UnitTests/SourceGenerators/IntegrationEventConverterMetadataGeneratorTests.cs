using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class IntegrationEventConverterMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_IntegrationEventConverterMetadataAttribute()
    {
        // TODO: 补充具体断言
        var assembly = typeof(IntegrationEventConverterMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventConverterMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.IntegrationEventConverterMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
    }
}
