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
        // 断言与实际生成的 EntityMetadataAttribute 保持一致
        Assert.Equal(7, attrs.Count);
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot"
            && a.IsAggregateRoot
            && a.SubEntities.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity","NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity2"})
            && a.MethodNames.SequenceEqual(new[]{"Create","ChangeName","PrivateMethod",".ctor"}));
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity"
            && !a.IsAggregateRoot
            && a.SubEntities.Length == 0
            && a.MethodNames.SequenceEqual(new[]{"ChangeTestEntityName",".ctor"}));
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity2"
            && !a.IsAggregateRoot
            && a.SubEntities.Length == 0
            && a.MethodNames.SequenceEqual(new[]{".ctor"}));
    }
}
