using System.Reflection;
using Xunit;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.Attributes;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class EdgeCaseTests
{
    [Fact]
    public void GetControllerNodes_WithNullCommandTypes_ShouldBeFiltered()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>
        {
            new ControllerMethodMetadataAttribute("TestController", "MethodWithNullCommands"),
            new ControllerMethodMetadataAttribute("TestController2", "MethodWithEmptyCommands"),
            new ControllerMethodMetadataAttribute("TestController3", "MethodWithValidCommands", "ValidCommand")
        };

        // Act
        var controllerNodes = CodeFlowAnalysisHelper.GetControllerNodes(attributes);
        var controllerMethodNodes = CodeFlowAnalysisHelper.GetControllerMethodNodes(attributes);

        // Assert
        // 只有TestController3应该被包含，因为它有有效的命令
        Assert.Single(controllerNodes);
        Assert.Equal("TestController3", controllerNodes[0].Id);
        
        Assert.Single(controllerMethodNodes);
        Assert.Equal("TestController3.MethodWithValidCommands", controllerMethodNodes[0].Id);
    }

    [Fact]
    public void GetCommandSenderNodes_WithNullCommandTypes_ShouldBeFiltered()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>
        {
            new CommandSenderMethodMetadataAttribute("TestSender", "MethodWithNullCommands"),
            new CommandSenderMethodMetadataAttribute("TestSender2", "MethodWithEmptyCommands"),
            new CommandSenderMethodMetadataAttribute("TestSender3", "MethodWithValidCommands", "ValidCommand")
        };

        // Act
        var commandSenderNodes = CodeFlowAnalysisHelper.GetCommandSenderNodes(attributes);
        var commandSenderMethodNodes = CodeFlowAnalysisHelper.GetCommandSenderMethodNodes(attributes);

        // Assert
        // 只有TestSender3应该被包含，因为它有有效的命令
        Assert.Single(commandSenderNodes);
        Assert.Equal("TestSender3", commandSenderNodes[0].Id);
        
        Assert.Single(commandSenderMethodNodes);
        Assert.Equal("TestSender3.MethodWithValidCommands", commandSenderMethodNodes[0].Id);
    }

    [Fact]
    public void GetControllerNodes_WithEmptyAttributes_ShouldReturnEmpty()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>();

        // Act
        var controllerNodes = CodeFlowAnalysisHelper.GetControllerNodes(attributes);
        var controllerMethodNodes = CodeFlowAnalysisHelper.GetControllerMethodNodes(attributes);

        // Assert
        Assert.Empty(controllerNodes);
        Assert.Empty(controllerMethodNodes);
    }

    [Fact]
    public void GetCommandSenderNodes_WithEmptyAttributes_ShouldReturnEmpty()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>();

        // Act
        var commandSenderNodes = CodeFlowAnalysisHelper.GetCommandSenderNodes(attributes);
        var commandSenderMethodNodes = CodeFlowAnalysisHelper.GetCommandSenderMethodNodes(attributes);

        // Assert
        Assert.Empty(commandSenderNodes);
        Assert.Empty(commandSenderMethodNodes);
    }

    [Fact]
    public void GetControllerNodes_WithDuplicateTypes_ShouldGroupCorrectly()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>
        {
            new ControllerMethodMetadataAttribute("SameController", "Method1", "Command1"),
            new ControllerMethodMetadataAttribute("SameController", "Method2", "Command2"),
            new ControllerMethodMetadataAttribute("SameController", "Method3", "Command3")
        };

        // Act
        var controllerNodes = CodeFlowAnalysisHelper.GetControllerNodes(attributes);
        var controllerMethodNodes = CodeFlowAnalysisHelper.GetControllerMethodNodes(attributes);

        // Assert
        // 应该只有一个Controller节点，但有三个方法节点
        Assert.Single(controllerNodes);
        Assert.Equal("SameController", controllerNodes[0].Id);
        
        Assert.Equal(3, controllerMethodNodes.Count);
        Assert.Contains(controllerMethodNodes, n => n.Id == "SameController.Method1");
        Assert.Contains(controllerMethodNodes, n => n.Id == "SameController.Method2");
        Assert.Contains(controllerMethodNodes, n => n.Id == "SameController.Method3");
    }

    [Fact]
    public void GetCommandSenderNodes_WithDuplicateTypes_ShouldGroupCorrectly()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>
        {
            new CommandSenderMethodMetadataAttribute("SameSender", "Method1", "Command1"),
            new CommandSenderMethodMetadataAttribute("SameSender", "Method2", "Command2"),
            new CommandSenderMethodMetadataAttribute("SameSender", "Method3", "Command3")
        };

        // Act
        var commandSenderNodes = CodeFlowAnalysisHelper.GetCommandSenderNodes(attributes);
        var commandSenderMethodNodes = CodeFlowAnalysisHelper.GetCommandSenderMethodNodes(attributes);

        // Assert
        // 应该只有一个CommandSender节点，但有三个方法节点
        Assert.Single(commandSenderNodes);
        Assert.Equal("SameSender", commandSenderNodes[0].Id);
        
        Assert.Equal(3, commandSenderMethodNodes.Count);
        Assert.Contains(commandSenderMethodNodes, n => n.Id == "SameSender.Method1");
        Assert.Contains(commandSenderMethodNodes, n => n.Id == "SameSender.Method2");
        Assert.Contains(commandSenderMethodNodes, n => n.Id == "SameSender.Method3");
    }

    [Fact]
    public void GetControllerNodes_WithMixedScenarios_ShouldFilterCorrectly()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>
        {
            // 有命令的方法
            new ControllerMethodMetadataAttribute("ValidController", "ValidMethod", "Command1"),
            // 没有命令的方法
            new ControllerMethodMetadataAttribute("InvalidController", "InvalidMethod"),
            // 空命令数组的方法
            new ControllerMethodMetadataAttribute("EmptyController", "EmptyMethod"),
            // 同一Controller的多个方法，有些有命令，有些没有
            new ControllerMethodMetadataAttribute("MixedController", "MethodWithCommand", "Command2"),
            new ControllerMethodMetadataAttribute("MixedController", "MethodWithoutCommand")
        };

        // Act
        var controllerNodes = CodeFlowAnalysisHelper.GetControllerNodes(attributes);
        var controllerMethodNodes = CodeFlowAnalysisHelper.GetControllerMethodNodes(attributes);

        // Assert
        // 应该有ValidController和MixedController，因为它们都有至少一个有命令的方法
        Assert.Equal(2, controllerNodes.Count);
        Assert.Contains(controllerNodes, n => n.Id == "ValidController");
        Assert.Contains(controllerNodes, n => n.Id == "MixedController");
        Assert.DoesNotContain(controllerNodes, n => n.Id == "InvalidController");
        Assert.DoesNotContain(controllerNodes, n => n.Id == "EmptyController");

        // 应该只有有命令的方法
        Assert.Equal(2, controllerMethodNodes.Count);
        Assert.Contains(controllerMethodNodes, n => n.Id == "ValidController.ValidMethod");
        Assert.Contains(controllerMethodNodes, n => n.Id == "MixedController.MethodWithCommand");
        Assert.DoesNotContain(controllerMethodNodes, n => n.Id == "InvalidController.InvalidMethod");
        Assert.DoesNotContain(controllerMethodNodes, n => n.Id == "EmptyController.EmptyMethod");
        Assert.DoesNotContain(controllerMethodNodes, n => n.Id == "MixedController.MethodWithoutCommand");
    }

    [Fact]
    public void GetClassName_ShouldHandleVariousInputs()
    {
        // 这是一个私有方法，我们通过反射来测试
        var type = typeof(CodeFlowAnalysisHelper);
        var method = type.GetMethod("GetClassName", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        // Test cases
        var testCases = new[]
        {
            ("Namespace.ClassName", "ClassName"),
            ("Very.Long.Namespace.ClassName", "ClassName"),
            ("ClassName", "ClassName"),
            ("", ""),
            ((string?)null, "")
        };

        foreach (var (input, expected) in testCases)
        {
            var result = method.Invoke(null, new object?[] { input }) as string;
            Assert.Equal(expected, result);
        }
    }

    [Fact]
    public void FilteringLogic_ShouldHandleNullAndEmptyArrays()
    {
        // Arrange
        var attributes = new List<MetadataAttribute>
        {
            // 测试各种边界情况
            new ControllerMethodMetadataAttribute("Test1", "Method1"), // 默认空数组
            new ControllerMethodMetadataAttribute("Test2", "Method2"), // 默认空数组
            new ControllerMethodMetadataAttribute("Test3", "Method3", ""), // 空字符串命令
            new ControllerMethodMetadataAttribute("Test4", "Method4", "ValidCommand"), // 有效命令
            new CommandSenderMethodMetadataAttribute("Sender1", "Method1"), // 默认空数组
            new CommandSenderMethodMetadataAttribute("Sender2", "Method2"), // 默认空数组
            new CommandSenderMethodMetadataAttribute("Sender3", "Method3", ""), // 空字符串命令
            new CommandSenderMethodMetadataAttribute("Sender4", "Method4", "ValidCommand"), // 有效命令
        };

        // Act
        var controllerNodes = CodeFlowAnalysisHelper.GetControllerNodes(attributes);
        var controllerMethodNodes = CodeFlowAnalysisHelper.GetControllerMethodNodes(attributes);
        var commandSenderNodes = CodeFlowAnalysisHelper.GetCommandSenderNodes(attributes);
        var commandSenderMethodNodes = CodeFlowAnalysisHelper.GetCommandSenderMethodNodes(attributes);

        // Assert
        // 只有Test4和Sender4应该被包含（因为空字符串命令也算是有效的）
        Assert.Equal(2, controllerNodes.Count);
        Assert.Contains(controllerNodes, n => n.Id == "Test3");
        Assert.Contains(controllerNodes, n => n.Id == "Test4");
        
        Assert.Equal(2, controllerMethodNodes.Count);
        Assert.Contains(controllerMethodNodes, n => n.Id == "Test3.Method3");
        Assert.Contains(controllerMethodNodes, n => n.Id == "Test4.Method4");

        Assert.Equal(2, commandSenderNodes.Count);
        Assert.Contains(commandSenderNodes, n => n.Id == "Sender3");
        Assert.Contains(commandSenderNodes, n => n.Id == "Sender4");
        
        Assert.Equal(2, commandSenderMethodNodes.Count);
        Assert.Contains(commandSenderMethodNodes, n => n.Id == "Sender3.Method3");
        Assert.Contains(commandSenderMethodNodes, n => n.Id == "Sender4.Method4");
    }
}
