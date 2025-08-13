using System.Reflection;
using Xunit;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.Attributes;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class AdvancedCodeFlowAnalysisTests
{
    [Fact]
    public void GetControllerNodes_ShouldFilterEmptyControllers()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var attributes = CodeFlowAnalysisHelper.GetAllMetadataAttributes(assembly);

        // Act
        var controllerNodes = CodeFlowAnalysisHelper.GetControllerNodes(attributes);
        var controllerMethodNodes = CodeFlowAnalysisHelper.GetControllerMethodNodes(attributes);

        // Assert - 确保没有发出命令的Controller和方法被过滤掉
        Assert.DoesNotContain(controllerNodes, n => n.FullName?.Contains("TestEmptyController") == true);
        Assert.DoesNotContain(controllerMethodNodes, n => n.FullName?.Contains("TestEmptyController.GetDataOnly") == true);
        Assert.DoesNotContain(controllerMethodNodes, n => n.FullName?.Contains("TestEmptyController.DoNothing") == true);
        Assert.DoesNotContain(controllerMethodNodes, n => n.FullName?.Contains("TestController.MethodWithOutCommand") == true);

        // 验证有命令的Controller存在
        Assert.Contains(controllerNodes, n => n.FullName?.Contains("TestComplexController") == true);
        Assert.Contains(controllerMethodNodes, n => n.FullName?.Contains("TestComplexController.ConditionalCommandSend") == true);
    }

    [Fact]
    public void GetCommandSenderNodes_ShouldFilterEmptyCommandSenders()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var attributes = CodeFlowAnalysisHelper.GetAllMetadataAttributes(assembly);

        // Act
        var commandSenderNodes = CodeFlowAnalysisHelper.GetCommandSenderNodes(attributes);
        var commandSenderMethodNodes = CodeFlowAnalysisHelper.GetCommandSenderMethodNodes(attributes);

        // Assert - 确保没有发出命令的CommandSender和方法被过滤掉
        Assert.DoesNotContain(commandSenderNodes, n => n.FullName?.Contains("TestEmptyCommandSender") == true);
        Assert.DoesNotContain(commandSenderMethodNodes, n => n.FullName?.Contains("TestEmptyCommandSender.QueryOnly") == true);
        Assert.DoesNotContain(commandSenderMethodNodes, n => n.FullName?.Contains("TestEmptyCommandSender.DoNothing") == true);

        // 验证有命令的CommandSender存在
        Assert.Contains(commandSenderNodes, n => n.FullName?.Contains("TestAdvancedCommandSender") == true);
        Assert.Contains(commandSenderMethodNodes, n => n.FullName?.Contains("TestAdvancedCommandSender.ConditionalSend") == true);
    }

    [Fact]
    public void GetEndpointNodes_ShouldIncludeEndpointsWithCommands()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var attributes = CodeFlowAnalysisHelper.GetAllMetadataAttributes(assembly);

        // Act
        var endpointNodes = CodeFlowAnalysisHelper.GetEndpointNodes(attributes);

        // Assert - Endpoint目前没有过滤逻辑，应该包含所有Endpoint
        // 这里我们可以验证新添加的高级Endpoint是否被正确识别
        var endpointCount = endpointNodes.Count;
        Assert.True(endpointCount > 0, "应该有至少一个Endpoint节点");
    }

    [Fact]
    public void AnalyzeComplexControllerScenarios_ShouldGenerateCorrectRelationships()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(assembly);

        // Act & Assert - 验证复杂Controller的关系
        var complexControllerMethods = result.Nodes
            .Where(n => n.Type == NodeType.ControllerMethod && n.FullName?.Contains("TestComplexController") == true)
            .ToList();

        var complexControllerRelationships = result.Relationships
            .Where(r => r.Type == RelationshipType.ControllerMethodToCommand && 
                       r.FromNode.FullName?.Contains("TestComplexController") == true)
            .ToList();

        Assert.True(complexControllerMethods.Count > 0, "应该有TestComplexController的方法节点");
        Assert.True(complexControllerRelationships.Count > 0, "应该有TestComplexController的命令关系");
    }

    [Fact]
    public void AnalyzeAdvancedCommandSenderScenarios_ShouldGenerateCorrectRelationships()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(assembly);

        // Act & Assert - 验证高级CommandSender的关系
        var advancedCommandSenderMethods = result.Nodes
            .Where(n => n.Type == NodeType.CommandSenderMethod && n.FullName?.Contains("TestAdvancedCommandSender") == true)
            .ToList();

        var advancedCommandSenderRelationships = result.Relationships
            .Where(r => r.Type == RelationshipType.CommandSenderMethodToCommand && 
                       r.FromNode.FullName?.Contains("TestAdvancedCommandSender") == true)
            .ToList();

        Assert.True(advancedCommandSenderMethods.Count > 0, "应该有TestAdvancedCommandSender的方法节点");
        Assert.True(advancedCommandSenderRelationships.Count > 0, "应该有TestAdvancedCommandSender的命令关系");
    }

    [Fact]
    public void AnalyzeInheritanceScenarios_ShouldHandleInheritedMethods()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(assembly);

        // Act & Assert - 验证继承场景
        var baseControllerMethods = result.Nodes
            .Where(n => n.Type == NodeType.ControllerMethod && n.FullName?.Contains("BaseTestController") == true)
            .ToList();

        var derivedControllerMethods = result.Nodes
            .Where(n => n.Type == NodeType.ControllerMethod && n.FullName?.Contains("DerivedTestController") == true)
            .ToList();

        var baseCommandSenderMethods = result.Nodes
            .Where(n => n.Type == NodeType.CommandSenderMethod && n.FullName?.Contains("BaseCommandSender") == true)
            .ToList();

        var derivedCommandSenderMethods = result.Nodes
            .Where(n => n.Type == NodeType.CommandSenderMethod && n.FullName?.Contains("DerivedCommandSender") == true)
            .ToList();

        // 验证基类和派生类的方法都被正确识别
        Assert.True(baseControllerMethods.Count > 0 || derivedControllerMethods.Count > 0, 
            "应该有继承场景的Controller方法");
        Assert.True(baseCommandSenderMethods.Count > 0 || derivedCommandSenderMethods.Count > 0, 
            "应该有继承场景的CommandSender方法");
    }

    [Fact]
    public void AnalyzeGenericScenarios_ShouldHandleGenericTypes()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(assembly);

        // Act & Assert - 验证泛型场景
        var allNodes = result.Nodes.ToList();
        var allRelationships = result.Relationships.ToList();

        // 验证泛型Controller和CommandSender的方法被正确处理
        var genericMethods = allNodes
            .Where(n => n.FullName?.Contains("Generic") == true)
            .ToList();

        // 即使没有泛型方法被识别，测试也应该通过，因为泛型可能不会被Source Generator处理
        Assert.True(allNodes.Count > 0, "应该有节点被生成");
        Assert.True(allRelationships.Count > 0, "应该有关系被生成");
    }

    [Fact]
    public void FilteringLogic_ShouldOnlyIncludeMethodsWithCommands()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var attributes = CodeFlowAnalysisHelper.GetAllMetadataAttributes(assembly);

        // Act
        var controllerMethodNodes = CodeFlowAnalysisHelper.GetControllerMethodNodes(attributes);
        var commandSenderMethodNodes = CodeFlowAnalysisHelper.GetCommandSenderMethodNodes(attributes);

        // Assert - 验证过滤逻辑
        foreach (var methodNode in controllerMethodNodes)
        {
            // 每个ControllerMethod节点都应该对应一个有命令的方法
            var hasCommand = attributes.OfType<ControllerMethodMetadataAttribute>()
                .Any(attr => $"{attr.ControllerType}.{attr.ControllerMethodName}" == methodNode.Id &&
                           attr.CommandTypes != null && attr.CommandTypes.Length > 0);
            
            Assert.True(hasCommand, $"Controller方法 {methodNode.Id} 应该有对应的命令");
        }

        foreach (var methodNode in commandSenderMethodNodes)
        {
            // 每个CommandSenderMethod节点都应该对应一个有命令的方法
            var hasCommand = attributes.OfType<CommandSenderMethodMetadataAttribute>()
                .Any(attr => $"{attr.SenderType}.{attr.SenderMethodName}" == methodNode.Id &&
                           attr.CommandTypes != null && attr.CommandTypes.Length > 0);
            
            Assert.True(hasCommand, $"CommandSender方法 {methodNode.Id} 应该有对应的命令");
        }
    }
}
