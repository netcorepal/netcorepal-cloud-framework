using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class CommandHandlerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_CommandHandlerMetadataAttribute()
    {
        // TODO: 补充具体断言
        var assembly = typeof(CommandHandlerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandHandlerMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandHandlerMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        Assert.NotEmpty(attrs);
        // 示例：断言数量
        Assert.Equal(14, attrs.Count); // 期望生成2个元数据，根据实际情况调整
        // 示例：断言具体内容
        Assert.Contains(attrs, a => a.HandlerType.Contains("CreateOrderCommandHandler") && a.CommandType.Contains("CreateOrderCommand") && a.EntityType.Contains("Order") && a.EntityMethodName == ".ctor");
        Assert.Contains(attrs, a => a.HandlerType.Contains("DeleteOrderCommandHandler") && a.CommandType.Contains("DeleteOrderCommand") && a.EntityType.Contains("Order") && a.EntityMethodName == "SoftDelete");
    }
}
