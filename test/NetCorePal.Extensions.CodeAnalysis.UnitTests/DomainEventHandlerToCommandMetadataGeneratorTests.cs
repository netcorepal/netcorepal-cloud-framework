using Xunit;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class DomainEventHandlerToCommandMetadataGeneratorTests
{
    // 移除 Generator_CanBeInstantiated 测试方法

    [Fact]
    public void Should_Generate_DomainEventHandlerToCommandMetadataAttribute()
    {
        var assembly = typeof(TestClasses.DomainEventHandlers.OrderCreatedDomainEventHandler).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventHandlerToCommandMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.DomainEventHandlerToCommandMetadataAttribute>()
            .ToList();
        var businessAttrs = attrs.Where(a => a.HandlerType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.")).ToList();
        Assert.Equal(2, businessAttrs.Count);
        Assert.Contains(businessAttrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.OrderCreatedDomainEventHandler" && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderCreatedDomainEvent" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateUserCommand"));
        Assert.Contains(businessAttrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.OrderPaidDomainEventHandler" && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderPaidDomainEvent" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ActivateUserCommand"));
        Assert.Contains(businessAttrs, a => a.HandlerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.OrderPaidDomainEventHandler" && a.EventType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.OrderPaidDomainEvent" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ChangeOrderNameCommand"));
    }
} 