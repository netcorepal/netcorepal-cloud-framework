using Xunit;
using System.Linq;
using System;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.SourceGenerators;

public class CommandMetadataGeneratorTests
{
    [Fact]
    public void Should_Generate_CommandMetadataAttribute()
    {
        var assembly = typeof(CommandMetadataGeneratorTests).Assembly;
        var attrs = assembly.GetCustomAttributes(typeof(NetCorePal.Extensions.CodeAnalysis.Attributes.CommandMetadataAttribute), false)
            .Cast<NetCorePal.Extensions.CodeAnalysis.Attributes.CommandMetadataAttribute>()
            .ToList();
        Assert.NotNull(attrs);
        // TestClasses 中所有命令类型
        var expected = new[]
        {
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateOrderCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.OrderPaidCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ChangeOrderNameCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.DeleteOrderCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.UpdateOrderItemQuantityCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CreateUserCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.ActivateUserCommand",
            "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.DeactivateUserCommand"
        };
        Assert.Equal(expected.Length, attrs.Count);
        foreach (var type in expected)
        {
            Assert.Contains(attrs, a => a.CommandType == type);
        }
    }
}
