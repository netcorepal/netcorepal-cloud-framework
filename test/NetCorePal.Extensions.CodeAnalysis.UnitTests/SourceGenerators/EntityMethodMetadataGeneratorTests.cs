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
        Assert.Equal(15, attrs.Count);
        
        // 测试实例方法
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot"
            && a.MethodName == "ChangeName"
            && a.EventTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRootNameChangedDomainEvent"})
            && a.CalledEntityMethods.Length == 0);
            
        // 测试私有方法
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot"
            && a.MethodName == "PrivateMethod"
            && a.EventTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestPrivateMethodDomainEvent"})
            && a.CalledEntityMethods.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity.ChangeTestEntityName"}));
            
        // 测试实体方法
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntity"
            && a.MethodName == "ChangeTestEntityName"
            && a.EventTypes.SequenceEqual(new[]{"NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestEntityNameChangedDomainEvent"})
            && a.CalledEntityMethods.Length == 0);
            
        // 测试构造函数
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot"
            && a.MethodName == ".ctor"
            && a.EventTypes.Length >= 0
            && a.CalledEntityMethods.Length >= 0);
            
        // 测试静态方法
        Assert.Contains(attrs, a => a.EntityType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.TestAggregateRoot"
            && a.MethodName == "Create"
            && a.EventTypes.Length == 0
            && a.CalledEntityMethods.Length == 0);
    }
}
