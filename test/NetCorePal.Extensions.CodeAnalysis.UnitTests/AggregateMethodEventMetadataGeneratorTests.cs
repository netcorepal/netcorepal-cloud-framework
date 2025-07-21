using Xunit;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class AggregateMethodEventMetadataGeneratorTests
{
    // 移除 Generator_CanBeInstantiated 测试方法

    [Fact]
    public void Should_Generate_AggregateMethodEventMetadataAttribute()
    {
        var assembly = typeof(TestClasses.Order).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.AggregateMethodEventMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.AggregateMethodEventMetadataAttribute>()
            .ToList();
        var businessAttrs = attrs.Where(a => a.AggregateType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order") || a.AggregateType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User")).ToList();
        // 输出实际 businessAttrs 详细内容，辅助分析
        foreach (var attr in businessAttrs)
        {
            Console.WriteLine($"AggregateType={attr.AggregateType}, MethodName={attr.MethodName}, EventTypes=[{string.Join(",", attr.EventTypes)}]");
        }
        // 动态断言：确保关键方法和事件都被生成
        Assert.Contains(businessAttrs, a =>
            a.AggregateType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order" &&
            a.MethodName == "MarkAsPaid" &&
            a.EventTypes.Any(e => e.EndsWith("OrderPaidDomainEvent")));
        Assert.Contains(businessAttrs, a =>
            a.AggregateType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order" &&
            a.MethodName == "ChangeName" &&
            a.EventTypes.Any(e => e.EndsWith("OrderNameChangedDomainEvent")));
        Assert.Contains(businessAttrs, a =>
            a.AggregateType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order" &&
            a.MethodName == "SoftDelete" &&
            a.EventTypes.Any(e => e.EndsWith("OrderDeletedDomainEvent")));
        Assert.Contains(businessAttrs, a =>
            a.AggregateType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order" &&
            a.MethodName == "AddOrderItem" &&
            a.EventTypes.Any(e => e.EndsWith("OrderItemAddedDomainEvent")));
        Assert.Contains(businessAttrs, a =>
            a.AggregateType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User" &&
            a.MethodName == "Activate" &&
            a.EventTypes.Any(e => e.EndsWith("UserActivatedDomainEvent")));
        Assert.Contains(businessAttrs, a =>
            a.AggregateType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User" &&
            a.MethodName == "Deactivate" &&
            a.EventTypes.Any(e => e.EndsWith("UserDeactivatedDomainEvent")));
        Assert.Contains(businessAttrs, a =>
            a.AggregateType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order" &&
            a.MethodName == "PayAndRename" &&
            a.EventTypes.Any(e => e.EndsWith("OrderPaidDomainEvent")) &&
            a.EventTypes.Any(e => e.EndsWith("OrderNameChangedDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == ".ctor" && a.AggregateType.Contains("Order") && a.EventTypes.Any(e => e.EndsWith("OrderCreatedDomainEvent")) && a.EventTypes.Any(e => e.EndsWith("UserCreatedDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == ".ctor" && a.AggregateType.Contains("User") && a.EventTypes.Any(e => e.EndsWith("UserCreatedDomainEvent")) && a.EventTypes.Any(e => e.EndsWith("OrderCreatedDomainEvent")));
        // 最后断言数量
        Assert.Equal(9, businessAttrs.Count);
    }
} 