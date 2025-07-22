using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class ControllerMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_ControllerMetadataAttribute()
    {
        var assembly = typeof(ControllerMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.ControllerMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.ControllerMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // TestClasses 中所有 Controller 方法与命令
        var expected = new[]
        {
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController", "CreateUser", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateUserCommand"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController", "ActivateUser", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ActivateUserCommand"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController", "DeactivateUser", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.DeactivateUserCommand"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController", "CreateOrder", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateOrderCommand"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController", "PayOrder", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.OrderPaidCommand"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController", "DeleteOrder", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.DeleteOrderCommand"),
            ("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.OrderController", "ChangeOrderName", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ChangeOrderNameCommand")
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var (controller, method, command) in expected)
        {
            Assert.Contains(attrs, a => a.ControllerType == controller && a.ControllerMethodName == method && a.CommandTypes.Contains(command));
        }
    }
}
