using System.Collections.Generic;
using System.Linq;
using NetCorePal.Extensions.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class MermaidVisualizerTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void GenerateArchitectureFlowChart_WithEmptyResult_ShouldProduceBasicMermaidDiagram()
    {
        // Arrange
        var emptyResult = new CodeFlowAnalysisResult();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateArchitectureFlowChart(emptyResult);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart TD", mermaidDiagram);
        Assert.Contains("Controllers", mermaidDiagram);
        Assert.Contains("Commands", mermaidDiagram);
        Assert.Contains("Entities", mermaidDiagram);

        testOutputHelper.WriteLine("=== Empty Architecture Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateArchitectureFlowChart_WithSampleData_ShouldProduceMermaidDiagram()
    {
        // Arrange
        var result = CreateSampleAnalysisResult();

        // Debug: Print the relationships first
        testOutputHelper.WriteLine("=== Relationships in Sample Data ===");
        foreach (var relationship in result.Relationships)
        {
            testOutputHelper.WriteLine($"{relationship.CallType}: {relationship.SourceType} -> {relationship.TargetType}");
        }
        testOutputHelper.WriteLine("");

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateArchitectureFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart TD", mermaidDiagram);
        Assert.Contains("Controllers", mermaidDiagram);
        Assert.Contains("Commands", mermaidDiagram);
        Assert.Contains("Entities", mermaidDiagram);
        Assert.Contains("OrderController", mermaidDiagram);
        Assert.Contains("CreateOrderCommand", mermaidDiagram);
        Assert.Contains("Order", mermaidDiagram);

        testOutputHelper.WriteLine("=== Architecture Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateCommandFlowChart_WithSampleData_ShouldProduceMermaidDiagram()
    {
        // Arrange
        var result = CreateSampleAnalysisResult();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateCommandFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart LR", mermaidDiagram);

        testOutputHelper.WriteLine("=== Command Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateEventFlowChart_WithSampleData_ShouldProduceMermaidDiagram()
    {
        // Arrange
        var result = CreateSampleAnalysisResult();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateEventFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart TD", mermaidDiagram);
        Assert.Contains("Domain Events", mermaidDiagram);

        testOutputHelper.WriteLine("=== Event Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateClassDiagram_WithSampleData_ShouldProduceMermaidDiagram()
    {
        // Arrange
        var result = CreateSampleAnalysisResult();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateClassDiagram(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("classDiagram", mermaidDiagram);

        testOutputHelper.WriteLine("=== Class Diagram ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateAllDiagrams_WithComplexSampleData_ShouldProduceValidDiagrams()
    {
        // Arrange - 创建复杂的示例数据
        var complexResult = CreateComplexSampleAnalysisResult();

        testOutputHelper.WriteLine("=== Complex Sample Architecture Flow Chart ===");
        var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(complexResult);
        testOutputHelper.WriteLine(architectureChart);
        testOutputHelper.WriteLine("");

        testOutputHelper.WriteLine("=== Complex Sample Command Flow Chart ===");
        var commandChart = MermaidVisualizer.GenerateCommandFlowChart(complexResult);
        testOutputHelper.WriteLine(commandChart);
        testOutputHelper.WriteLine("");

        testOutputHelper.WriteLine("=== Complex Sample Event Flow Chart ===");
        var eventChart = MermaidVisualizer.GenerateEventFlowChart(complexResult);
        testOutputHelper.WriteLine(eventChart);
        testOutputHelper.WriteLine("");

        testOutputHelper.WriteLine("=== Complex Sample Class Diagram ===");
        var classChart = MermaidVisualizer.GenerateClassDiagram(complexResult);
        testOutputHelper.WriteLine(classChart);

        // Assert all diagrams are not empty
        Assert.NotEmpty(architectureChart);
        Assert.NotEmpty(commandChart);
        Assert.NotEmpty(eventChart);
        Assert.NotEmpty(classChart);
    }

    [Fact]
    public void MermaidVisualizer_ShouldHandleSpecialCharactersInNames()
    {
        // Arrange - 创建包含特殊字符的数据
        var result = new CodeFlowAnalysisResult
        {
            Controllers = new List<ControllerInfo>
            {
                new() { Name = "Test\"Controller", FullName = "Test.Controllers.Test\"Controller", Methods = new List<string> { "Get<T>", "Post[Array]" } }
            },
            Commands = new List<CommandInfo>
            {
                new() { Name = "Create\"Command", FullName = "Test.Commands.Create\"Command", Properties = new List<string>() }
            }
        };

        // Act
        var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(result);
        var commandChart = MermaidVisualizer.GenerateCommandFlowChart(result);
        var eventChart = MermaidVisualizer.GenerateEventFlowChart(result);
        var classChart = MermaidVisualizer.GenerateClassDiagram(result);

        // Assert - 应该不会抛出异常，并且生成有效的图表
        Assert.NotEmpty(architectureChart);
        Assert.NotEmpty(commandChart);
        Assert.NotEmpty(eventChart);
        Assert.NotEmpty(classChart);

        testOutputHelper.WriteLine("=== Special Characters Architecture Chart ===");
        testOutputHelper.WriteLine(architectureChart);
    }

    [Fact]
    public void GenerateAllDiagrams_With_This_Assembly()
    {
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // 输出关系详情用于人工检查
        Console.WriteLine("Relationships in analysis result:");
        foreach (var relationship in result.Relationships)
        {
            Console.WriteLine($"  {relationship.CallType}: {relationship.SourceType} -> {relationship.TargetType}");
        }
        Console.WriteLine($"Total relationships: {result.Relationships.Count}");
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(result);

        // Print the diagram to see what's actually generated
        Console.WriteLine("Generated Architecture Flowchart:");
        Console.WriteLine(architectureChart);
        Console.WriteLine("\n" + new string('=', 80) + "\n");
    }

    [Fact]
    public void GenerateCommandChainFlowCharts_With_This_Assembly()
    {
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // 使用Json输出关系详情用于人工检查
        Console.WriteLine("Relationships in analysis result:");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));

        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);

        // Print the diagrams to see what's actually generated
        Console.WriteLine("Generated Command Chain Flow Charts:");
        Console.WriteLine($"Total chains generated: {chainDiagrams.Count}");
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            Console.WriteLine($"Chain: {chainName}");
            Console.WriteLine("```mermaid");
            Console.WriteLine(diagram);
            Console.WriteLine("```");
            Console.WriteLine("");
        }
        Console.WriteLine("\n" + new string('=', 80) + "\n");
    }

    [Fact]
    public void GenerateMultiChainFlowChart_With_This_Assembly()
    {
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var r=  result.Relationships.Where(p=>p.CallType.Contains("MethodToDomainEvent"))
            .ToList(); // 触发LINQ查询，确保关系被处理
        // 输出关系详情用于人工检查
        Console.WriteLine("Relationships in analysis result:");
        foreach (var relationship in result.Relationships)
        {
            Console.WriteLine($"  {relationship.CallType}: {relationship.SourceType} -> {relationship.TargetType}");
        }
        Console.WriteLine($"Total relationships: {result.Relationships.Count}");
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        var architectureChart = MermaidVisualizer.GenerateMultiChainFlowChart(result);

        // Print the diagram to see what's actually generated
        Console.WriteLine("Generated Architecture Flowchart:");
        Console.WriteLine(architectureChart);
        Console.WriteLine("\n" + new string('=', 80) + "\n");
    }

    [Fact]
    public void GenerateAllChainFlowCharts_With_This_Assembly()
    {
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var r = result.Relationships.Where(p => p.CallType.Contains("MethodToDomainEvent"))
            .ToList(); // 触发LINQ查询，确保关系被处理
        
        // 输出关系详情用于人工检查
        Console.WriteLine("Relationships in analysis result:");
        foreach (var relationship in result.Relationships)
        {
            Console.WriteLine($"  {relationship.CallType}: {relationship.SourceType} -> {relationship.TargetType}");
        }
        Console.WriteLine($"Total relationships: {result.Relationships.Count}");
        Console.WriteLine("\n" + new string('-', 80) + "\n");

        var chainFlowCharts = MermaidVisualizer.GenerateAllChainFlowCharts(result);

        // Print each individual chain diagram
        Console.WriteLine($"Generated {chainFlowCharts.Count} Individual Chain Flowcharts:");
        Console.WriteLine(new string('=', 80));
        
        for (int i = 0; i < chainFlowCharts.Count; i++)
        {
            Console.WriteLine($"\n--- Chain {i + 1} ---");
            Console.WriteLine(chainFlowCharts[i]);
            Console.WriteLine(new string('-', 60));
        }
        
        Console.WriteLine("\n" + new string('=', 80) + "\n");
        
        // Assert that we have at least one chain
        Assert.NotEmpty(chainFlowCharts);
        
        // Assert that each chain is a valid Mermaid diagram
        foreach (var chart in chainFlowCharts)
        {
            Assert.NotEmpty(chart);
            Assert.StartsWith("flowchart TD", chart);
            Assert.Contains("%%", chart); // Should contain comments
        }
    }

    [Fact]
    public void GenerateArchitectureFlowChart_WithMultipleEventHandlers_ShouldVisualizeProperly()
    {
        // Arrange - 创建包含多个事件处理器的测试数据
        var result = CreateMultipleEventHandlersAnalysisResult();

        // Debug: Print the relationships first
        testOutputHelper.WriteLine("=== Relationships in Multiple Event Handlers Data ===");
        foreach (var relationship in result.Relationships)
        {
            testOutputHelper.WriteLine($"{relationship.CallType}: {relationship.SourceType} -> {relationship.TargetType}");
        }
        testOutputHelper.WriteLine("");

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateArchitectureFlowChart(result);
        var eventFlowChart = MermaidVisualizer.GenerateEventFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.NotEmpty(eventFlowChart);

        // 验证图表包含我们期望的元素
        Assert.Contains("UserRegisteredDomainEvent", mermaidDiagram);
        Assert.Contains("UserRegisteredIntegrationEvent", mermaidDiagram);
        Assert.Contains("UserRegisteredWelcomeEmailHandler", mermaidDiagram);
        Assert.Contains("UserRegisteredStatisticsHandler", mermaidDiagram);
        Assert.Contains("UserRegisteredDefaultSettingsHandler", mermaidDiagram);
        Assert.Contains("UserRegisteredCrmSyncHandler", mermaidDiagram);
        Assert.Contains("UserRegisteredMarketingHandler", mermaidDiagram);
        Assert.Contains("UserRegisteredPushNotificationHandler", mermaidDiagram);

        testOutputHelper.WriteLine("=== Architecture Flow Chart with Multiple Event Handlers ===");
        testOutputHelper.WriteLine(mermaidDiagram);
        testOutputHelper.WriteLine("");

        testOutputHelper.WriteLine("=== Event Flow Chart with Multiple Event Handlers ===");
        testOutputHelper.WriteLine(eventFlowChart);

        // 验证关系数量是否正确
        // 应该有3个领域事件处理器关系 + 3个集成事件处理器关系 + 1个转换器关系
        var domainEventToHandlerRelationships = result.Relationships.Count(r => r.CallType == "DomainEventToHandler");
        var integrationEventToHandlerRelationships = result.Relationships.Count(r => r.CallType == "IntegrationEventToHandler");
        var domainEventToIntegrationEventRelationships = result.Relationships.Count(r => r.CallType == "DomainEventToIntegrationEvent");

        Assert.Equal(3, domainEventToHandlerRelationships);
        Assert.Equal(3, integrationEventToHandlerRelationships);
        Assert.Equal(1, domainEventToIntegrationEventRelationships);
    }

    [Fact]
    public void GenerateArchitectureFlowChart_WithConstructorAndStaticMethodCalls_ShouldVisualizeProperly()
    {
        // Arrange - 创建包含构造函数和静态方法调用的测试数据
        var result = CreateConstructorAndStaticMethodAnalysisResult();

        // Debug: Print the relationships first
        testOutputHelper.WriteLine("=== Relationships in Constructor and Static Method Test Data ===");
        foreach (var relationship in result.Relationships)
        {
            testOutputHelper.WriteLine($"{relationship.CallType}: {relationship.SourceType} -> {relationship.TargetType} ({relationship.TargetMethod})");
        }
        testOutputHelper.WriteLine("");

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateArchitectureFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);

        // 验证构造函数调用关系
        Assert.Contains(".ctor", mermaidDiagram);
        Assert.Contains("CreateDefault", mermaidDiagram);

        // 验证生成的图表包含预期的关系
        Assert.Contains("executes .ctor", mermaidDiagram);
        Assert.Contains("executes CreateDefault", mermaidDiagram);

        testOutputHelper.WriteLine("=== Constructor and Static Method Architecture Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateCommandChainFlowCharts_WithSampleData_ShouldProduceChainDiagrams()
    {
        // Arrange
        var result = CreateSampleAnalysisResult();

        // Act
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);

        // Assert
        Assert.NotEmpty(chainDiagrams);

        testOutputHelper.WriteLine("=== Command Chain Flow Charts ===");
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            testOutputHelper.WriteLine($"Chain: {chainName}");
            testOutputHelper.WriteLine("```mermaid");
            testOutputHelper.WriteLine(diagram);
            testOutputHelper.WriteLine("```");
            testOutputHelper.WriteLine("");

            // 验证每个图表的基本结构
            Assert.Contains("flowchart TD", diagram);
            Assert.NotEmpty(diagram);
        }
    }

    [Fact]
    public void GenerateCommandChainFlowCharts_WithComplexData_ShouldProduceMultipleChains()
    {
        // Arrange
        var result = CreateComplexSampleAnalysisResult();

        // Act
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);

        // Assert
        Assert.NotEmpty(chainDiagrams);

        testOutputHelper.WriteLine("=== Complex Command Chain Flow Charts ===");
        testOutputHelper.WriteLine($"Total chains generated: {chainDiagrams.Count}");
        testOutputHelper.WriteLine("");

        foreach (var (chainName, diagram) in chainDiagrams)
        {
            testOutputHelper.WriteLine($"Chain: {chainName}");
            testOutputHelper.WriteLine("```mermaid");
            testOutputHelper.WriteLine(diagram);
            testOutputHelper.WriteLine("```");
            testOutputHelper.WriteLine("");

            // 验证每个图表的基本结构
            Assert.Contains("flowchart TD", diagram);
            Assert.NotEmpty(diagram);
        }
    }

    [Fact]
    public void GenerateCommandChainFlowCharts_WithRealAssembly_ShouldProduceChains()
    {
        // Arrange
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // Act
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);

        // Assert - 可能为空，因为测试程序集可能没有命令发送者
        testOutputHelper.WriteLine("=== Real Assembly Command Chain Flow Charts ===");
        testOutputHelper.WriteLine($"Total chains generated: {chainDiagrams.Count}");
        testOutputHelper.WriteLine("");

        foreach (var (chainName, diagram) in chainDiagrams)
        {
            testOutputHelper.WriteLine($"Chain: {chainName}");
            testOutputHelper.WriteLine("```mermaid");
            testOutputHelper.WriteLine(diagram);
            testOutputHelper.WriteLine("```");
            testOutputHelper.WriteLine("");
        }
    }

    [Fact]
    public void GenerateMultiChainFlowChart_WithSampleData_ShouldProduceSingleDiagramWithMultipleChains()
    {
        // Arrange
        var result = CreateSampleAnalysisResult();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateMultiChainFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart TD", mermaidDiagram);
        Assert.Contains("subgraph", mermaidDiagram);
        Assert.Contains("Chain1:", mermaidDiagram);

        testOutputHelper.WriteLine("=== Multi-Chain Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
        testOutputHelper.WriteLine("");
    }

    [Fact]
    public void GenerateMultiChainFlowChart_WithComplexData_ShouldShowAllChainsInOneGraph()
    {
        // Arrange
        var result = CreateComplexSampleAnalysisResult();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateMultiChainFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart TD", mermaidDiagram);
        Assert.Contains("subgraph", mermaidDiagram);

        // 验证包含多个链路
        Assert.Contains("Chain1:", mermaidDiagram);
        Assert.Contains("Chain2:", mermaidDiagram);

        testOutputHelper.WriteLine("=== Complex Multi-Chain Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
        testOutputHelper.WriteLine("");
    }

    [Fact]
    public void CompareAllMermaidVisualizationMethods_ShouldShowDifferentPerspectives()
    {
        // Arrange
        var result = CreateComplexSampleAnalysisResult();

        testOutputHelper.WriteLine("=== 对比所有可视化方法 ===");
        testOutputHelper.WriteLine("");

        // 1. 完整架构图
        testOutputHelper.WriteLine("### 1. 完整架构图（显示所有组件和关系）");
        var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(result);
        testOutputHelper.WriteLine("```mermaid");
        testOutputHelper.WriteLine(architectureChart);
        testOutputHelper.WriteLine("```");
        testOutputHelper.WriteLine("");

        // 2. 分链路图
        testOutputHelper.WriteLine("### 2. 分链路图（每个链路一个独立图表）");
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);
        testOutputHelper.WriteLine($"总共 {chainDiagrams.Count} 个独立链路：");
        foreach (var (chainName, diagram) in chainDiagrams.Take(2)) // 只显示前2个作为示例
        {
            testOutputHelper.WriteLine($"#### {chainName}");
            testOutputHelper.WriteLine("```mermaid");
            testOutputHelper.WriteLine(diagram);
            testOutputHelper.WriteLine("```");
            testOutputHelper.WriteLine("");
        }

        // 3. 多链路合并图
        testOutputHelper.WriteLine("### 3. 多链路合并图（一张图显示多个链路）");
        var multiChainChart = MermaidVisualizer.GenerateMultiChainFlowChart(result);
        testOutputHelper.WriteLine("```mermaid");
        testOutputHelper.WriteLine(multiChainChart);
        testOutputHelper.WriteLine("```");
        testOutputHelper.WriteLine("");

        // Assert all methods work
        Assert.NotEmpty(architectureChart);
        Assert.NotEmpty(chainDiagrams);
        Assert.NotEmpty(multiChainChart);
    }

    private static CodeFlowAnalysisResult CreateSampleAnalysisResult()
    {
        return new CodeFlowAnalysisResult
        {
            Controllers = new List<ControllerInfo>
            {
                new() { Name = "OrderController", FullName = "NetCorePal.Web.Controllers.OrderController", Methods = new List<string> { "Get", "Post", "SetPaid" } }
            },
            Commands = new List<CommandInfo>
            {
                new() { Name = "CreateOrderCommand", FullName = "NetCorePal.Web.Application.Commands.CreateOrderCommand", Properties = new List<string>() },
                new() { Name = "OrderPaidCommand", FullName = "NetCorePal.Web.Application.Commands.OrderPaidCommand", Properties = new List<string>() }
            },
            Entities = new List<EntityInfo>
            {
                new() { Name = "Order", FullName = "NetCorePal.Web.Domain.Order", IsAggregateRoot = true, Methods = new List<string> { "OrderPaid", "SoftDelete" } }
            },
            DomainEvents = new List<DomainEventInfo>
            {
                new() { Name = "OrderCreatedDomainEvent", FullName = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", Properties = new List<string>() }
            },
            IntegrationEvents = new List<IntegrationEventInfo>
            {
                new() { Name = "OrderCreatedIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent" }
            },
            DomainEventHandlers = new List<DomainEventHandlerInfo>
            {
                new() { Name = "OrderCreatedDomainEventHandler", FullName = "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", HandledEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", Commands = new List<string>() }
            },
            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent", Commands = new List<string>() }
            },
            IntegrationEventConverters = new List<IntegrationEventConverterInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.OrderCreatedIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent" }
            },
            Relationships = new List<CallRelationship>
            {
                new("NetCorePal.Web.Controllers.OrderController", "Post", "NetCorePal.Web.Application.Commands.CreateOrderCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Application.Commands.OrderPaidCommand", "Handle", "NetCorePal.Web.Domain.Order", "OrderPaid", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler")
            }
        };
    }

    private static CodeFlowAnalysisResult CreateComplexSampleAnalysisResult()
    {
        return new CodeFlowAnalysisResult
        {
            Controllers = new List<ControllerInfo>
            {
                new() { Name = "OrderController", FullName = "NetCorePal.Web.Controllers.OrderController", Methods = new List<string> { "Get", "Post", "GetById", "SetPaid", "SetOrderItemName", "DeleteOrder" } },
                new() { Name = "UserController", FullName = "NetCorePal.Web.Controllers.UserController", Methods = new List<string> { "CreateUser", "Login", "UpdateProfile" } },
                new() { Name = "ProductController", FullName = "NetCorePal.Web.Controllers.ProductController", Methods = new List<string> { "GetProducts", "CreateProduct" } }
            },
            Commands = new List<CommandInfo>
            {
                new() { Name = "CreateOrderCommand", FullName = "NetCorePal.Web.Application.Commands.CreateOrderCommand", Properties = new List<string>() },
                new() { Name = "OrderPaidCommand", FullName = "NetCorePal.Web.Application.Commands.OrderPaidCommand", Properties = new List<string>() },
                new() { Name = "DeleteOrderCommand", FullName = "NetCorePal.Web.Application.Commands.DeleteOrderCommand", Properties = new List<string>() },
                new() { Name = "ChangeOrderNameCommand", FullName = "NetCorePal.Web.Application.Commands.ChangeOrderNameCommand", Properties = new List<string>() },
                new() { Name = "SetOrderItemNameCommand", FullName = "NetCorePal.Web.Application.Commands.SetOrderItemNameCommand", Properties = new List<string>() },
                new() { Name = "CreateUserCommand", FullName = "NetCorePal.Web.Application.Commands.CreateUserCommand", Properties = new List<string>() },
                new() { Name = "ActivateUserCommand", FullName = "NetCorePal.Web.Application.Commands.ActivateUserCommand", Properties = new List<string>() }
            },
            Entities = new List<EntityInfo>
            {
                new() { Name = "Order", FullName = "NetCorePal.Web.Domain.Order", IsAggregateRoot = true, Methods = new List<string> { "OrderPaid", "SoftDelete", "ChangeItemName" } },
                new() { Name = "User", FullName = "NetCorePal.Web.Domain.User", IsAggregateRoot = true, Methods = new List<string> { "UpdateProfile", "Activate" } },
                new() { Name = "Product", FullName = "NetCorePal.Web.Domain.Product", IsAggregateRoot = false, Methods = new List<string> { "UpdatePrice" } }
            },
            DomainEvents = new List<DomainEventInfo>
            {
                new() { Name = "OrderCreatedDomainEvent", FullName = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", Properties = new List<string>() },
                new() { Name = "OrderPaidDomainEvent", FullName = "NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", Properties = new List<string>() },
                new() { Name = "UserCreatedDomainEvent", FullName = "NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", Properties = new List<string>() }
            },
            IntegrationEvents = new List<IntegrationEventInfo>
            {
                new() { Name = "OrderCreatedIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent" },
                new() { Name = "OrderPaidIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEvents.OrderPaidIntegrationEvent" },
                new() { Name = "OrderDeletedIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEvents.OrderDeletedIntegrationEvent" },
                new() { Name = "UserCreatedIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent" }
            },
            DomainEventHandlers = new List<DomainEventHandlerInfo>
            {
                new() { Name = "OrderCreatedDomainEventHandler", FullName = "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", HandledEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.CreateUserCommand" } },
                new() { Name = "OrderPaidDomainEventHandler", FullName = "NetCorePal.Web.Application.DomainEventHandlers.OrderPaidDomainEventHandler", HandledEventType = "NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.ActivateUserCommand" } }
            },
            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.CreateUserCommand" } },
                new() { Name = "OrderPaidIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderPaidIntegrationEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.ChangeOrderNameCommand" } },
                new() { Name = "OrderDeletedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderDeletedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderDeletedIntegrationEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.DeleteOrderCommand" } },
                new() { Name = "UserCreatedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.CreateUserCommand" } }
            },
            IntegrationEventConverters = new List<IntegrationEventConverterInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.OrderCreatedIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent" },
                new() { Name = "OrderPaidIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.OrderPaidIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderPaidIntegrationEvent" },
                new() { Name = "OrderDeletedIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.OrderDeletedIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.OrderDeletedDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderDeletedIntegrationEvent" },
                new() { Name = "UserCreatedIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.UserCreatedIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent" }
            },
            Relationships = new List<CallRelationship>
            {
                // Controller to Command relationships
                new("NetCorePal.Web.Controllers.OrderController", "Post", "NetCorePal.Web.Application.Commands.CreateOrderCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.OrderController", "SetPaid", "NetCorePal.Web.Application.Commands.OrderPaidCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.OrderController", "DeleteOrder", "NetCorePal.Web.Application.Commands.DeleteOrderCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.UserController", "CreateUser", "NetCorePal.Web.Application.Commands.CreateUserCommand", "", "MethodToCommand"),
                
                // Command to Aggregate relationships
                new("NetCorePal.Web.Application.Commands.CreateOrderCommand", "Handle", "NetCorePal.Web.Domain.Order", ".ctor", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Application.Commands.OrderPaidCommand", "Handle", "NetCorePal.Web.Domain.Order", "OrderPaid", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Application.Commands.DeleteOrderCommand", "Handle", "NetCorePal.Web.Domain.Order", "SoftDelete", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Application.Commands.ChangeOrderNameCommand", "Handle", "NetCorePal.Web.Domain.Order", "ChangeName", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Application.Commands.SetOrderItemNameCommand", "Handle", "NetCorePal.Web.Domain.Order", "ChangeItemName", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Application.Commands.CreateUserCommand", "Handle", "NetCorePal.Web.Domain.User", ".ctor", "CommandToAggregateMethod"),
                
                // Aggregate Method to Domain Event relationships (only for events that should be produced by specific methods)
                new("NetCorePal.Web.Domain.Order", ".ctor", "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "MethodToDomainEvent"),
                new("NetCorePal.Web.Domain.Order", "OrderPaid", "NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", "", "MethodToDomainEvent"),
                new("NetCorePal.Web.Domain.Order", "SoftDelete", "NetCorePal.Web.Domain.DomainEvents.OrderDeletedDomainEvent", "", "MethodToDomainEvent"),
                new("NetCorePal.Web.Domain.User", ".ctor", "NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", "", "MethodToDomainEvent"),
                
                // Domain Event to Handler relationships
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", "", "NetCorePal.Web.Application.DomainEventHandlers.OrderPaidDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                
                // Domain Event to Integration Event relationships (through converters)
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", "", "NetCorePal.Web.Application.IntegrationEvents.OrderPaidIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderDeletedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEvents.OrderDeletedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                
                // Integration Event to Handler relationships
                new("NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", "HandleAsync", "IntegrationEventToHandler"),
                new("NetCorePal.Web.Application.IntegrationEvents.OrderPaidIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", "HandleAsync", "IntegrationEventToHandler"),
                new("NetCorePal.Web.Application.IntegrationEvents.OrderDeletedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderDeletedIntegrationEventHandler", "HandleAsync", "IntegrationEventToHandler"),
                new("NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEventHandler", "HandleAsync", "IntegrationEventToHandler"),
                
                // Integration Event Handler to Command relationships
                new("NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", "HandleAsync", "NetCorePal.Web.Application.Commands.CreateUserCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Application.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", "HandleAsync", "NetCorePal.Web.Application.Commands.ChangeOrderNameCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Application.IntegrationEventHandlers.OrderDeletedIntegrationEventHandler", "HandleAsync", "NetCorePal.Web.Application.Commands.DeleteOrderCommand", "", "MethodToCommand")
            }
        };
    }

    private static CodeFlowAnalysisResult CreateMultipleEventHandlersAnalysisResult()
    {
        return new CodeFlowAnalysisResult
        {
            Controllers = new List<ControllerInfo>
            {
                new() { Name = "UserController", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController", Methods = new List<string> { "CompleteUserRegistration" } }
            },
            Commands = new List<CommandInfo>
            {
                new() { Name = "CompleteUserRegistrationCommand", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CompleteUserRegistrationCommand", Properties = new List<string>() }
            },
            Entities = new List<EntityInfo>
            {
                new() { Name = "User", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User", IsAggregateRoot = true, Methods = new List<string> { "CompleteRegistration" } }
            },
            DomainEvents = new List<DomainEventInfo>
            {
                new() { Name = "UserRegisteredDomainEvent", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", Properties = new List<string>() }
            },
            IntegrationEvents = new List<IntegrationEventInfo>
            {
                new() { Name = "UserRegisteredIntegrationEvent", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent" }
            },
            DomainEventHandlers = new List<DomainEventHandlerInfo>
            {
                new() { Name = "UserRegisteredWelcomeEmailHandler", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.UserRegisteredWelcomeEmailHandler", HandledEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", Commands = new List<string>() },
                new() { Name = "UserRegisteredStatisticsHandler", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.UserRegisteredStatisticsHandler", HandledEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", Commands = new List<string>() },
                new() { Name = "UserRegisteredDefaultSettingsHandler", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.UserRegisteredDefaultSettingsHandler", HandledEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", Commands = new List<string>() }
            },
            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo>
            {
                new() { Name = "UserRegisteredCrmSyncHandler", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.UserRegisteredCrmSyncHandler", HandledEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent", Commands = new List<string>() },
                new() { Name = "UserRegisteredMarketingHandler", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.UserRegisteredMarketingHandler", HandledEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent", Commands = new List<string>() },
                new() { Name = "UserRegisteredPushNotificationHandler", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.UserRegisteredPushNotificationHandler", HandledEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent", Commands = new List<string>() }
            },
            IntegrationEventConverters = new List<IntegrationEventConverterInfo>
            {
                new() { Name = "UserRegisteredIntegrationEventConverter", FullName = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventConverters.UserRegisteredIntegrationEventConverter", DomainEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", IntegrationEventType = "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent" }
            },
            Relationships = new List<CallRelationship>
            {
                // Controller to Command relationship
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers.UserController", "CompleteUserRegistration", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CompleteUserRegistrationCommand", "", "MethodToCommand"),
                
                // Command to Aggregate relationship
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands.CompleteUserRegistrationCommand", "Handle", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.User", "CompleteRegistration", "CommandToAggregateMethod"),
                
                // Domain Event to multiple Domain Event Handlers relationships
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", "", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.UserRegisteredWelcomeEmailHandler", "HandleAsync", "DomainEventToHandler"),
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", "", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.UserRegisteredStatisticsHandler", "HandleAsync", "DomainEventToHandler"),
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", "", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers.UserRegisteredDefaultSettingsHandler", "HandleAsync", "DomainEventToHandler"),
                
                // Domain Event to Integration Event relationship
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents.UserRegisteredDomainEvent", "", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                
                // Integration Event to multiple Integration Event Handlers relationships
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent", "", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.UserRegisteredCrmSyncHandler", "Subscribe", "IntegrationEventToHandler"),
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent", "", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.UserRegisteredMarketingHandler", "Subscribe", "IntegrationEventToHandler"),
                new("NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents.UserRegisteredIntegrationEvent", "", "NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers.UserRegisteredPushNotificationHandler", "Subscribe", "IntegrationEventToHandler")
            }
        };
    }

    private static CodeFlowAnalysisResult CreateConstructorAndStaticMethodAnalysisResult()
    {
        return new CodeFlowAnalysisResult
        {
            Controllers = new List<ControllerInfo>
            {
                new() { Name = "OrderController", FullName = "Test.Controllers.OrderController", Methods = new List<string> { "CreateOrder", "CreateDefaultOrder" } }
            },
            Commands = new List<CommandInfo>
            {
                new() { Name = "CreateOrderCommand", FullName = "Test.Commands.CreateOrderCommand", Properties = new List<string>() },
                new() { Name = "CreateDefaultOrderCommand", FullName = "Test.Commands.CreateDefaultOrderCommand", Properties = new List<string>() }
            },
            Entities = new List<EntityInfo>
            {
                new() { Name = "Order", FullName = "Test.Entities.Order", Methods = new List<string> { ".ctor", "CreateDefault", "MarkAsPaid" } }
            },
            DomainEvents = new List<DomainEventInfo>(),
            IntegrationEvents = new List<IntegrationEventInfo>(),
            DomainEventHandlers = new List<DomainEventHandlerInfo>(),
            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo>(),
            IntegrationEventConverters = new List<IntegrationEventConverterInfo>(),
            Relationships = new List<CallRelationship>
            {
                // Controller to Command relationships
                new("Test.Controllers.OrderController", "CreateOrder", "Test.Commands.CreateOrderCommand", "", "MethodToCommand"),
                new("Test.Controllers.OrderController", "CreateDefaultOrder", "Test.Commands.CreateDefaultOrderCommand", "", "MethodToCommand"),
                
                // Command to Aggregate relationships - 构造函数调用
                new("Test.Commands.CreateOrderCommand", "Handle", "Test.Entities.Order", ".ctor", "CommandToAggregateMethod"),
                
                // Command to Aggregate relationships - 静态方法调用
                new("Test.Commands.CreateDefaultOrderCommand", "Handle", "Test.Entities.Order", "CreateDefault", "CommandToAggregateMethod")
            }
        };
    }
}
