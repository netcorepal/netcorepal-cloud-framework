using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class EntityMethodMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_EntityMethodMetadataAttribute()
    {
        var assembly = typeof(EntityMethodMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.EntityMethodMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.EntityMethodMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // 断言与实际生成的 EntityMethodMetadataAttribute 保持一致
        Assert.Equal(3, attrs.Count);
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot"
            && a.MethodName == "ChangeName"
            && a.EventTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEvent"})
            && a.CalledEntityMethods.Length == 0);
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot"
            && a.MethodName == "PrivateMethod"
            && a.EventTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodDomainEvent"})
            && a.CalledEntityMethods.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity.ChangeTestEntityName"}));
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity"
            && a.MethodName == "ChangeTestEntityName"
            && a.EventTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntityNameChangedDomainEvent"})
            && a.CalledEntityMethods.Length == 0);
    }
}
