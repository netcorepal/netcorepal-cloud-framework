using Xunit;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using NetCorePal.Extensions.CodeAnalysis.SourceGenerators;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class ControllerMethodToCommandMetadataGeneratorTests
{
    // 移除 Generator_CanBeInstantiated 测试方法

    [Fact]
    public void Should_Generate_ControllerMethodToCommandMetadataAttribute()
    {
        var assembly = typeof(TestClasses.Controllers.UserController).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.ControllerMethodToCommandMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.ControllerMethodToCommandMetadataAttribute>()
            .ToList();
        var businessAttrs = attrs.Where(a => a.ControllerType.StartsWith("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.")).ToList();
        Assert.Equal(7, businessAttrs.Count);
        Assert.Contains(businessAttrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController" && a.MethodName == "CreateUser" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateUserCommand"));
        Assert.Contains(businessAttrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController" && a.MethodName == "ActivateUser" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ActivateUserCommand"));
        Assert.Contains(businessAttrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController" && a.MethodName == "DeactivateUser" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.DeactivateUserCommand"));
        Assert.Contains(businessAttrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController" && a.MethodName == "CreateOrder" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateOrderCommand"));
        Assert.Contains(businessAttrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController" && a.MethodName == "PayOrder" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.OrderPaidCommand"));
        Assert.Contains(businessAttrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController" && a.MethodName == "DeleteOrder" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.DeleteOrderCommand"));
        Assert.Contains(businessAttrs, a => a.ControllerType == "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController" && a.MethodName == "ChangeOrderName" && a.CommandTypes.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ChangeOrderNameCommand"));
    }
} 