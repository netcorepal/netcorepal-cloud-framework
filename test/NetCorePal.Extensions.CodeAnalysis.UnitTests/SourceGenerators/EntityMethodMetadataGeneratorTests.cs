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
        // TestClasses 中所有实体方法（Order/User/OrderItem 的所有领域事件方法和聚合内方法调用）
        var expected = new[]
        {
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order", "MarkAsPaid"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order", "ChangeName"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order", "SoftDelete"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order", "AddOrderItem"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order", "PayAndRename"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User", "Activate"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User", "Deactivate"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.OrderItem", "UpdateQuantity")
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var (entity, method) in expected)
        {
            Assert.Contains(attrs, a => a.EntityType == entity && a.MethodName == method);
        }
    }
}
