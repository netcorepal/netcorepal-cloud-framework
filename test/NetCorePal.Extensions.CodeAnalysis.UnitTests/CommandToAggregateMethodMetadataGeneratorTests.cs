using Xunit;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class CommandToAggregateMethodMetadataGeneratorTests
{
    // 移除 Generator_CanBeInstantiated 测试方法

    [Fact]
    public void Should_Generate_CommandToAggregateMethodMetadataAttribute()
    {
        var assembly = typeof(TestClasses.CommandHandlers.CreateOrderCommandHandler).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandToAggregateMethodMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandToAggregateMethodMetadataAttribute>()
            .ToList();
        var businessAttrs = attrs.Where(a => a.AggregateType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Order") || a.AggregateType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User")).ToList();
        // 输出实际 businessAttrs 便于人工确认
        foreach (var attr in businessAttrs)
        {
            Console.WriteLine($"AggregateType={attr.AggregateType}, CommandType={attr.CommandType}, MethodName={attr.MethodName}, EventTypes=[{string.Join(",", attr.EventTypes)}]");
        }
        Assert.Equal(5, businessAttrs.Count);
        Assert.Contains(businessAttrs, a => a.MethodName == "MarkAsPaid" && a.EventTypes.Contains("MarkAsPaidDomainEvent"));
        Assert.Contains(businessAttrs, a => a.MethodName == "SoftDelete" && a.EventTypes.Contains("SoftDeleteDomainEvent"));
        Assert.Contains(businessAttrs, a => a.MethodName == "ChangeName" && a.EventTypes.Contains("ChangeNameDomainEvent"));
        Assert.Contains(businessAttrs, a => a.MethodName == "Activate" && a.EventTypes.Contains("ActivateDomainEvent"));
        Assert.Contains(businessAttrs, a => a.MethodName == "Deactivate" && a.EventTypes.Contains("DeactivateDomainEvent"));
    }
} 