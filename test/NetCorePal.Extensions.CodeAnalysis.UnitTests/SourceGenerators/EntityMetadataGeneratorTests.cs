using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class EntityMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_EntityMetadataAttribute()
    {
        var assembly = typeof(EntityMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.EntityMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.EntityMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // 只断言 User 和 Order 类型必须被包含
        Assert.Contains(attrs, a => a.EntityType.EndsWith("User"));
        Assert.Contains(attrs, a => a.EntityType.EndsWith("Order"));
        Assert.Contains(attrs, a => a.EntityType.EndsWith("OrderItem"));
        Assert.True(attrs.Count == 3);
    }
}
