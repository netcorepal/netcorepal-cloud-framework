using Xunit;
using System;
using System.Linq;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class DomainEventToHandlerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_DomainEventToHandlerMetadataAttribute()
    {
        // 获取当前测试程序集
        var assembly = typeof(DomainEventToHandlerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventToHandlerMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventToHandlerMetadataAttribute>()
            .ToList();
        // 输出实际 attrs 便于人工确认
        foreach (var attr in attrs)
        {
            Console.WriteLine($"DomainEventType={attr.DomainEventType}, HandlerTypes=[{string.Join(", ", attr.HandlerTypes)}]");
        }
        // 断言至少有一个领域事件与处理器关系被生成
        Assert.NotEmpty(attrs);

        // 更准确断言：验证特定领域事件与处理器的关系
        // 假设测试数据中有如下领域事件和处理器
        Assert.Contains(attrs, a => a.DomainEventType.Contains("OrderCreatedDomainEvent") && a.HandlerTypes.Any(h => h.Contains("OrderCreatedDomainEventHandler")));
        Assert.Contains(attrs, a => a.DomainEventType.Contains("OrderPaidDomainEvent") && a.HandlerTypes.Any(h => h.Contains("OrderPaidDomainEventHandler")));
    }
}
