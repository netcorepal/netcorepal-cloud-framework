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
        // 输出实际 businessAttrs 便于人工确认
        foreach (var attr in businessAttrs)
        {
            Console.WriteLine($"AggregateType={attr.AggregateType}, MethodName={attr.MethodName}, EventTypes=[{string.Join(",", attr.EventTypes)}]");
        }
        Assert.Equal(7, businessAttrs.Count);
        Assert.Contains(businessAttrs, a => a.MethodName == "MarkAsPaid" && a.EventTypes.Any(e => e.Contains("MarkAsPaidDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == "ChangeName" && a.EventTypes.Any(e => e.Contains("ChangeNameDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == "SoftDelete" && a.EventTypes.Any(e => e.Contains("SoftDeleteDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == "AddOrderItem" && a.EventTypes.Any(e => e.Contains("AddOrderItemDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == "Activate" && a.EventTypes.Any(e => e.Contains("ActivateDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == "Deactivate" && a.EventTypes.Any(e => e.Contains("DeactivateDomainEvent")));
        Assert.Contains(businessAttrs, a => a.MethodName == "UpdateQuantity" && a.EventTypes.Contains("OrderItemQuantityUpdatedDomainEvent"));
        Assert.Contains(businessAttrs, a => a.MethodName == "PayAndRename" && a.EventTypes.Any(e => e.Contains("PayAndRenameDomainEvent")));
    }
} 