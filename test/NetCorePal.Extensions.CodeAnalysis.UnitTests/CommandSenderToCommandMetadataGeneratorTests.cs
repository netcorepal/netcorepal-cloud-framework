using Xunit;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class CommandSenderToCommandMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_CommandSenderToCommandMetadataAttribute()
    {
        var assembly = typeof(TestClasses.Services.OrderProcessingService).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandSenderToCommandMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandSenderToCommandMetadataAttribute>()
            .ToList();
        var businessAttrs = attrs.Where(a => a.SenderType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Services.")).ToList();
        Assert.Equal(3, businessAttrs.Count);
        Assert.Contains(businessAttrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Services.OrderProcessingService" && a.MethodName == "ProcessNewOrder" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateOrderCommand"));
        Assert.Contains(businessAttrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Services.OrderProcessingService" && a.MethodName == "ProcessBatchPayments" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.OrderPaidCommand"));
        Assert.Contains(businessAttrs, a => a.SenderType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Services.OrderProcessingService" && a.MethodName == "ProcessOrderModification" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ChangeOrderNameCommand"));
    }
} 