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
        // Arrange
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // Act
        var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(result);

        // Assert
        Assert.NotEmpty(architectureChart);
        Assert.Contains("flowchart TD", architectureChart);
        
        // 验证基本节点分组存在
        Assert.Contains("%% Controllers", architectureChart);
        Assert.Contains("%% Commands", architectureChart);
        Assert.Contains("%% Entities", architectureChart);
        Assert.Contains("%% Domain Events", architectureChart);
        Assert.Contains("%% Integration Events", architectureChart);
        
        // 验证样式定义存在
        Assert.Contains("classDef controller", architectureChart);
        Assert.Contains("classDef command", architectureChart);
        Assert.Contains("classDef entity", architectureChart);

        testOutputHelper.WriteLine("Generated Architecture Flowchart:");
        testOutputHelper.WriteLine(architectureChart);
        testOutputHelper.WriteLine($"Chart length: {architectureChart.Length} characters");
    }

    [Fact]
    public void GenerateCommandChainFlowCharts_With_This_Assembly()
    {
        // Arrange
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // Act
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);

        // Assert
        Assert.NotNull(chainDiagrams);
        
        // 验证返回的是字典结构
        Assert.IsAssignableFrom<List<(string ChainName, string MermaidDiagram)>>(chainDiagrams);
        
        // 如果有链路，验证每个链路的基本结构
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            Assert.NotNull(chainName);
            Assert.NotEmpty(chainName);
            Assert.NotNull(diagram);
            Assert.NotEmpty(diagram);
            Assert.Contains("flowchart TD", diagram);
            Assert.Contains("%%", diagram); // 应该包含注释
        }

        testOutputHelper.WriteLine($"Generated {chainDiagrams.Count} Command Chain Flow Charts");
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            testOutputHelper.WriteLine($"Chain: {chainName} (Length: {diagram.Length} chars)");
            Assert.True(diagram.Length > 20, $"Chain diagram for {chainName} seems too short");
        }
    }

    [Fact]
    public void GenerateMultiChainFlowChart_With_This_Assembly()
    {
        // Arrange
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // Act
        var multiChainChart = MermaidVisualizer.GenerateMultiChainFlowChart(result);

        // Assert
        Assert.NotNull(multiChainChart);
        Assert.NotEmpty(multiChainChart);
        Assert.Contains("flowchart TD", multiChainChart);
        
        // 验证多链路图的基本结构
        if (result.Relationships.Any(r => r.CallType == "MethodToCommand"))
        {
            // 如果有命令关系，应该包含子图
            Assert.Contains("subgraph", multiChainChart);
            Assert.Contains("Chain", multiChainChart);
        }
        
        // 验证样式定义
        Assert.Contains("classDef", multiChainChart);

        testOutputHelper.WriteLine("Generated Multi-Chain Flow Chart:");
        testOutputHelper.WriteLine($"Chart length: {multiChainChart.Length} characters");
        testOutputHelper.WriteLine($"Contains subgraphs: {multiChainChart.Contains("subgraph")}");
        testOutputHelper.WriteLine($"Total relationships: {result.Relationships.Count}");
    }

    [Fact]
    public void GenerateAllChainFlowCharts_With_This_Assembly()
    {
        // Arrange
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // Act
        var chainFlowCharts = MermaidVisualizer.GenerateAllChainFlowCharts(result);

        // Assert
        Assert.NotNull(chainFlowCharts);
        Assert.IsAssignableFrom<List<(string ChainName, string Diagram)>>(chainFlowCharts);
        
        // 验证每个独立链路图的结构
        foreach (var (chainName, diagram) in chainFlowCharts)
        {
            Assert.NotNull(chainName);
            Assert.NotEmpty(chainName);
            Assert.NotNull(diagram);
            Assert.NotEmpty(diagram);
            Assert.Contains("flowchart TD", diagram);
            Assert.Contains("%%", diagram); // 应该包含注释
            Assert.Contains("classDef", diagram); // 应该包含样式定义
            Assert.True(diagram.Length > 50, "Chain flowchart seems too short to be meaningful");
        }

        testOutputHelper.WriteLine($"Generated {chainFlowCharts.Count} Individual Chain Flowcharts");
        
        // 验证每个图表都是有效的 Mermaid 图表
        for (int i = 0; i < chainFlowCharts.Count; i++)
        {
            var (chainName, diagram) = chainFlowCharts[i];
            testOutputHelper.WriteLine($"Chain {i + 1} ({chainName}): {diagram.Length} characters");
            
            // 验证基本的 Mermaid 语法元素
            Assert.Contains("flowchart TD", diagram);
            Assert.True(diagram.Split('\n').Length > 3, $"Chain {i + 1} should have multiple lines");
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
        
        // 验证每个图表的基本结构
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            Assert.NotNull(chainName);
            Assert.NotEmpty(chainName);
            Assert.NotNull(diagram);
            Assert.NotEmpty(diagram);
            Assert.Contains("flowchart TD", diagram);
            Assert.Contains("%%", diagram); // 应该包含注释
            Assert.Contains("classDef", diagram); // 应该包含样式定义
            Assert.True(diagram.Length > 50, $"Chain diagram for {chainName} seems too short");
        }
        
        // 基于 CreateSampleAnalysisResult 的数据，验证特定的链路
        var chainNames = chainDiagrams.Select(c => c.ChainName).ToList();
        var expectedChains = new[] { "OrderController -> CreateOrderCommand", "OrderController -> OrderPaidCommand" };
        
        // 检查是否包含预期的链路
        foreach (var expectedChain in expectedChains)
        {
            Assert.Contains(expectedChain, chainNames);
        }

        testOutputHelper.WriteLine("=== Command Chain Flow Charts ===");
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
    public void GenerateCommandChainFlowCharts_WithComplexData_ShouldProduceMultipleChains()
    {
        // Arrange
        var result = CreateComplexSampleAnalysisResult();

        // Act
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);

        // Assert
        Assert.NotEmpty(chainDiagrams);
        
        // 验证复杂数据应该产生多个链路
        Assert.Equal(8, chainDiagrams.Count);
        
        // 验证每个图表的基本结构
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            Assert.NotNull(chainName);
            Assert.NotEmpty(chainName);
            Assert.NotNull(diagram);
            Assert.NotEmpty(diagram);
            Assert.Contains("flowchart TD", diagram);
            Assert.Contains("%%", diagram); // 应该包含注释
            Assert.Contains("classDef", diagram); // 应该包含样式定义
            Assert.True(diagram.Length > 50, $"Chain diagram for {chainName} seems too short");
        }
        
        // 基于 CreateComplexSampleAnalysisResult 的数据，验证特定的链路
        var chainNames = chainDiagrams.Select(c => c.ChainName).ToList();
        var expectedChains = new[] { "OrderController -> CreateOrderCommand", "OrderController -> OrderPaidCommand", "OrderController -> DeleteOrderCommand", "UserController -> CreateUserCommand" };
        
        // 检查是否包含预期的链路
        foreach (var expectedChain in expectedChains)
        {
            Assert.Contains(expectedChain, chainNames);
        }

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
        }
    }

    [Fact]
    public void GenerateCommandChainFlowCharts_WithRealAssembly_ShouldProduceChains()
    {
        // Arrange
        var result = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // Act
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);

        // Assert
        Assert.NotNull(chainDiagrams);
        Assert.IsAssignableFrom<List<(string ChainName, string MermaidDiagram)>>(chainDiagrams);
        
        // 验证每个链路图的基本结构
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            Assert.NotNull(chainName);
            Assert.NotEmpty(chainName);
            Assert.NotNull(diagram);
            Assert.NotEmpty(diagram);
            Assert.Contains("flowchart TD", diagram);
            Assert.Contains("%%", diagram); // 应该包含注释
            Assert.Contains("classDef", diagram); // 应该包含样式定义
            Assert.True(diagram.Length > 30, $"Chain diagram for {chainName} seems too short");
        }
        
        // 基于实际程序集的数据，验证链路的命名格式
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            // 链路名应该包含箭头符号（表示调用关系）
            Assert.Contains("->", chainName);
            
            // 链路名应该包含 Controller、Handler 或 Endpoint（控制器、事件处理器或端点）
            Assert.True(chainName.Contains("Controller") || chainName.Contains("Handler") || chainName.Contains("Endpoint"), 
                $"Chain name '{chainName}' should contain either 'Controller', 'Handler', or 'Endpoint'");
        }

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
        Assert.Contains("OrderController", mermaidDiagram);
        
        // 验证多链路图的结构
        Assert.Contains("classDef", mermaidDiagram); // 应该包含样式定义
        Assert.Contains("%%", mermaidDiagram); // 应该包含注释
        
        // 验证样式类别
        Assert.Contains("classDef controller", mermaidDiagram);
        Assert.Contains("classDef command", mermaidDiagram);
        Assert.Contains("classDef entity", mermaidDiagram);
        
        // 验证图表包含预期的组件
        Assert.Contains("OrderController", mermaidDiagram);
        Assert.Contains("CreateOrderCommand", mermaidDiagram);
        Assert.Contains("OrderPaidCommand", mermaidDiagram);
        Assert.Contains("Order", mermaidDiagram);
        
        // 验证链路子图的结构
        var subgraphCount = mermaidDiagram.Split("subgraph").Length - 1;
        Assert.Equal(2, subgraphCount);

        testOutputHelper.WriteLine("=== Multi-Chain Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
        testOutputHelper.WriteLine($"Subgraph count: {subgraphCount}");
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
        Assert.Contains("OrderController", mermaidDiagram);
        Assert.Contains("UserController", mermaidDiagram);
        
        // 验证多链路图的结构
        Assert.Contains("classDef", mermaidDiagram); // 应该包含样式定义
        Assert.Contains("%%", mermaidDiagram); // 应该包含注释
        
        // 验证样式类别
        Assert.Contains("classDef controller", mermaidDiagram);
        Assert.Contains("classDef command", mermaidDiagram);
        Assert.Contains("classDef entity", mermaidDiagram);
        
        // 验证图表包含预期的组件
        Assert.Contains("OrderController", mermaidDiagram);
        Assert.Contains("UserController", mermaidDiagram);
        Assert.Contains("CreateOrderCommand", mermaidDiagram);
        Assert.Contains("CreateUserCommand", mermaidDiagram);
        Assert.Contains("Order", mermaidDiagram);
        Assert.Contains("User", mermaidDiagram);
        
        // 验证链路子图的结构
        var subgraphCount = mermaidDiagram.Split("subgraph").Length - 1;
        Assert.Equal(4, subgraphCount);

        testOutputHelper.WriteLine("=== Complex Multi-Chain Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
        testOutputHelper.WriteLine($"Subgraph count: {subgraphCount}");
        testOutputHelper.WriteLine("");
        
        // 调试：显示所有链路信息以便理解为什么只有4个子图而不是8个
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);
        testOutputHelper.WriteLine($"Individual chain count: {chainDiagrams.Count}");
        testOutputHelper.WriteLine("Individual chains:");
        foreach (var (chainName, _) in chainDiagrams)
        {
            testOutputHelper.WriteLine($"  - {chainName}");
        }
    }

    [Fact]
    public void CompareAllMermaidVisualizationMethods_ShouldShowDifferentPerspectives()
    {
        // Arrange
        var result = CreateComplexSampleAnalysisResult();

        // Act
        var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(result);
        var chainDiagrams = MermaidVisualizer.GenerateCommandChainFlowCharts(result);
        var multiChainChart = MermaidVisualizer.GenerateMultiChainFlowChart(result);

        // Assert all methods work
        Assert.NotEmpty(architectureChart);
        Assert.NotEmpty(chainDiagrams);
        Assert.NotEmpty(multiChainChart);
        
        // 验证架构图包含所有组件
        Assert.Contains("%% Controllers", architectureChart);
        Assert.Contains("%% Commands", architectureChart);
        Assert.Contains("%% Entities", architectureChart);
        Assert.Contains("%% Domain Events", architectureChart);
        Assert.Contains("%% Integration Events", architectureChart);
        
        // 验证链路图数量和质量
        Assert.Equal(8, chainDiagrams.Count);
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            Assert.Contains("flowchart TD", diagram);
            Assert.Contains("%%", diagram);
        }
        
        // 验证多链路图结构
        Assert.Contains("flowchart TD", multiChainChart);
        Assert.Contains("subgraph", multiChainChart);
        Assert.Contains("OrderController", multiChainChart);
        Assert.Contains("UserController", multiChainChart);
        
        // 验证每种图表的长度差异
        Assert.True(architectureChart.Length > 500, "Architecture chart should be substantial");
        Assert.True(multiChainChart.Length > 200, "Multi-chain chart should be substantial");
        
        // 验证各图表的独特性
        Assert.NotEqual(architectureChart, multiChainChart);
        foreach (var (chainName, diagram) in chainDiagrams)
        {
            Assert.NotEqual(architectureChart, diagram);
            Assert.NotEqual(multiChainChart, diagram);
        }

        testOutputHelper.WriteLine("=== 对比所有可视化方法 ===");
        testOutputHelper.WriteLine("");

        // 1. 完整架构图
        testOutputHelper.WriteLine("### 1. 完整架构图（显示所有组件和关系）");
        testOutputHelper.WriteLine($"Length: {architectureChart.Length} characters");
        testOutputHelper.WriteLine("```mermaid");
        testOutputHelper.WriteLine(architectureChart);
        testOutputHelper.WriteLine("```");
        testOutputHelper.WriteLine("");

        // 2. 分链路图
        testOutputHelper.WriteLine("### 2. 分链路图（每个链路一个独立图表）");
        testOutputHelper.WriteLine($"总共 {chainDiagrams.Count} 个独立链路：");
        foreach (var (chainName, diagram) in chainDiagrams.Take(2)) // 只显示前2个作为示例
        {
            testOutputHelper.WriteLine($"#### {chainName} (Length: {diagram.Length} characters)");
            testOutputHelper.WriteLine("```mermaid");
            testOutputHelper.WriteLine(diagram);
            testOutputHelper.WriteLine("```");
            testOutputHelper.WriteLine("");
        }

        // 3. 多链路合并图
        testOutputHelper.WriteLine("### 3. 多链路合并图（一张图显示多个链路）");
        testOutputHelper.WriteLine($"Length: {multiChainChart.Length} characters");
        testOutputHelper.WriteLine("```mermaid");
        testOutputHelper.WriteLine(multiChainChart);
        testOutputHelper.WriteLine("```");
        testOutputHelper.WriteLine("");
    }

    [Fact]
    public void GenerateVisualizationHtml_WithSampleData_ShouldProduceCompleteHtmlPage()
    {
        // Arrange
        var analysisResult = CreateSampleAnalysisResult();

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult, "测试架构图");

        // Assert
        Assert.NotNull(htmlContent);
        Assert.NotEmpty(htmlContent);
        
        // 验证HTML基本结构
        Assert.Contains("<!DOCTYPE html>", htmlContent);
        Assert.Contains("<html lang=\"zh-CN\">", htmlContent);
        Assert.Contains("<title>测试架构图</title>", htmlContent);
        Assert.Contains("mermaid@10.6.1", htmlContent);
        
        // 验证包含必要的CSS样式
        Assert.Contains(".container", htmlContent);
        Assert.Contains(".sidebar", htmlContent);
        Assert.Contains(".main-content", htmlContent);
        Assert.Contains(".diagram-container", htmlContent);
        
        // 验证包含导航结构
        Assert.Contains("架构图导航", htmlContent);
        Assert.Contains("完整架构流程图", htmlContent);
        Assert.Contains("命令流程图", htmlContent);
        Assert.Contains("事件流程图", htmlContent);
        Assert.Contains("类图", htmlContent);
        Assert.Contains("单独链路流程图", htmlContent);
        
        // 验证JavaScript功能
        Assert.Contains("mermaid.initialize", htmlContent);
        Assert.Contains("analysisResult", htmlContent);
        Assert.Contains("diagrams", htmlContent);
        Assert.Contains("commandChains", htmlContent);
        Assert.Contains("allChainFlowCharts", htmlContent);
        Assert.Contains("initializePage", htmlContent);
        Assert.Contains("showDiagram", htmlContent);
        Assert.Contains("showIndividualChain", htmlContent);
        Assert.Contains("renderMermaidDiagram", htmlContent);
        
        // 验证包含分析结果数据
        Assert.Contains("OrderController", htmlContent);
        Assert.Contains("CreateOrderCommand", htmlContent);
        Assert.Contains("Order", htmlContent);
        Assert.Contains("OrderCreatedDomainEvent", htmlContent);
        
        // 验证包含图表数据
        Assert.Contains("flowchart TD", htmlContent); // 架构图
        Assert.Contains("flowchart LR", htmlContent); // 命令流程图
        Assert.Contains("classDiagram", htmlContent); // 类图
        
        testOutputHelper.WriteLine("=== Generated HTML Page ===");
        testOutputHelper.WriteLine($"HTML 内容长度: {htmlContent.Length} 字符");
        testOutputHelper.WriteLine("HTML页面生成成功，包含所有必要的组件和数据");
        
        // 验证HTML是否可以保存到文件
        Assert.True(htmlContent.Length > 10000, "生成的HTML内容应该足够详细");
    }

    [Fact]
    public void GenerateVisualizationHtml_WithComplexData_ShouldIncludeAllDiagramTypes()
    {
        // Arrange
        var analysisResult = CreateComplexSampleAnalysisResult();

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult);

        // Assert
        Assert.NotNull(htmlContent);
        
        // 验证包含复杂数据的所有组件
        Assert.Contains("OrderController", htmlContent);
        Assert.Contains("CreateOrderCommand", htmlContent);
        Assert.Contains("Order", htmlContent);
        Assert.Contains("OrderCreatedDomainEvent", htmlContent);
        Assert.Contains("OrderCreatedDomainEventHandler", htmlContent);
        Assert.Contains("OrderCreatedIntegrationEvent", htmlContent);
        Assert.Contains("OrderCreatedIntegrationEventHandler", htmlContent);
        Assert.Contains("OrderCreatedIntegrationEventConverter", htmlContent);
        
        // 验证生成的命令链路
        var commandChainCount = htmlContent.Split("commandChains = [")[1].Split("];")[0].Split("name:").Length - 1;
        Assert.True(commandChainCount > 0, "应该包含命令链路数据");
        
        // 验证JavaScript数据结构完整性
        Assert.Contains("controllers: [", htmlContent);
        Assert.Contains("commands: [", htmlContent);
        Assert.Contains("entities: [", htmlContent);
        Assert.Contains("domainEvents: [", htmlContent);
        Assert.Contains("integrationEvents: [", htmlContent);
        Assert.Contains("domainEventHandlers: [", htmlContent);
        Assert.Contains("integrationEventHandlers: [", htmlContent);
        Assert.Contains("integrationEventConverters: [", htmlContent);
        Assert.Contains("relationships: [", htmlContent);
        
        testOutputHelper.WriteLine("=== Complex Data HTML Generation ===");
        testOutputHelper.WriteLine($"HTML 内容长度: {htmlContent.Length} 字符");
        testOutputHelper.WriteLine($"估计命令链路数量: {commandChainCount}");
        testOutputHelper.WriteLine("复杂数据HTML页面生成成功");
    }

    [Fact]
    public void GenerateVisualizationHtml_WithEmptyData_ShouldProduceValidHtml()
    {
        // Arrange
        var analysisResult = new CodeFlowAnalysisResult
        {
            Controllers = [],
            Commands = [],
            Entities = [],
            DomainEvents = [],
            IntegrationEvents = [],
            DomainEventHandlers = [],
            IntegrationEventHandlers = [],
            IntegrationEventConverters = [],
            Relationships = []
        };

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult, "空数据测试");

        // Assert
        Assert.NotNull(htmlContent);
        Assert.NotEmpty(htmlContent);
        
        // 即使没有数据，也应该有完整的HTML结构
        Assert.Contains("<!DOCTYPE html>", htmlContent);
        Assert.Contains("空数据测试", htmlContent);
        Assert.Contains("mermaid.initialize", htmlContent);
        Assert.Contains("controllers: [", htmlContent);
        Assert.Contains("commands: [", htmlContent);
        
        // 验证空数组
        Assert.Contains("controllers: [", htmlContent);
        Assert.Contains("commands: [", htmlContent);
        Assert.Contains("entities: [", htmlContent);
        
        // 验证JavaScript数组结构（检查是否有空的数组结构）
        var controllersStart = htmlContent.IndexOf("controllers: [");
        var controllersEnd = htmlContent.IndexOf("],", controllersStart);
        Assert.True(controllersStart != -1 && controllersEnd != -1, "应该包含controllers数组");
        
        var commandsStart = htmlContent.IndexOf("commands: [");
        var commandsEnd = htmlContent.IndexOf("],", commandsStart);
        Assert.True(commandsStart != -1 && commandsEnd != -1, "应该包含commands数组");
        
        testOutputHelper.WriteLine("=== Empty Data HTML Generation ===");
        testOutputHelper.WriteLine("空数据情况下HTML页面生成成功，结构完整");
    }

    [Fact]
    public void GenerateVisualizationHtml_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var analysisResult = new CodeFlowAnalysisResult
        {
            Controllers = [
                new ControllerInfo { Name = "Test\"Controller", FullName = "MyApp.Controllers.Test\"Controller", Methods = ["Test'Method"] }
            ],
            Commands = [
                new CommandInfo { Name = "Test<Command>", FullName = "MyApp.Commands.Test<Command>" }
            ],
            Entities = [],
            DomainEvents = [],
            IntegrationEvents = [],
            DomainEventHandlers = [],
            IntegrationEventHandlers = [],
            IntegrationEventConverters = [],
            Relationships = []
        };

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult);

        // Assert
        Assert.NotNull(htmlContent);
        
        // 验证特殊字符被正确转义
        Assert.Contains("Test\\\"Controller", htmlContent); // JavaScript转义
        Assert.Contains("Test&quot;Controller", htmlContent); // HTML转义或包含在某些地方
        Assert.Contains("Test\\'Method", htmlContent); // JavaScript单引号转义
        Assert.Contains("Test&lt;Command&gt;", htmlContent); // HTML标签转义
        
        // 确保没有未转义的特殊字符导致语法错误
        Assert.DoesNotContain("Test\"Controller\",", htmlContent); // 应该被转义
        Assert.DoesNotContain("Test<Command>", htmlContent); // 应该被转义
        
        testOutputHelper.WriteLine("=== Special Characters Escaping Test ===");
        testOutputHelper.WriteLine("特殊字符转义测试通过");
    }

    [Fact]
    public void GenerateVisualizationHtml_For_This_Assembly()
    {
        // Arrange
        var analysisResult = AnalysisResultAggregator.Aggregate(typeof(MermaidVisualizerTests).Assembly);

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult);

        testOutputHelper.WriteLine("=== HTML Generation Test ===");
        testOutputHelper.WriteLine(htmlContent);

        // write html file to this project
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "MermaidDiagram.html");
        File.WriteAllText(filePath, htmlContent);

        // Assert
        Assert.NotNull(htmlContent);
        Assert.Contains("controllers: [", htmlContent);
        Assert.Contains("commands: [", htmlContent);
        Assert.Contains("entities: [", htmlContent);
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
                new("NetCorePal.Web.Controllers.OrderController", "SetPaid", "NetCorePal.Web.Application.Commands.OrderPaidCommand", "", "MethodToCommand"),
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
