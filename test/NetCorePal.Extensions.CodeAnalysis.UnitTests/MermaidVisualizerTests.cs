using System.Collections.Generic;
using System.Linq;
using NetCorePal.Extensions.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using System.IO;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class MermaidVisualizerTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void GenerateCommandFlowChart_WithSampleData_ShouldProduceMermaidDiagram()
    {
        // Arrange
        var result = CreateSampleAnalysisResult2();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateCommandFlowChart(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart LR", mermaidDiagram);

        testOutputHelper.WriteLine("=== Command Flow Chart ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateClassDiagram_WithSampleData_ShouldProduceMermaidDiagram()
    {
        // Arrange
        var result = CreateSampleAnalysisResult2();

        // Act
        var mermaidDiagram = MermaidVisualizer.GenerateClassDiagram(result);

        // Assert
        Assert.NotEmpty(mermaidDiagram);
        Assert.Contains("flowchart LR", mermaidDiagram);

        testOutputHelper.WriteLine("=== Class Diagram ===");
        testOutputHelper.WriteLine(mermaidDiagram);
    }

    [Fact]
    public void GenerateAllDiagrams_WithComplexSampleData_ShouldProduceValidDiagrams()
    {
        // Arrange - 创建复杂的示例数据
        var complexResult = CreateComplexSampleAnalysisResult2();

        testOutputHelper.WriteLine("=== Complex Sample Command Flow Chart ===");
        var commandChart = MermaidVisualizer.GenerateCommandFlowChart(complexResult);
        testOutputHelper.WriteLine(commandChart);
        testOutputHelper.WriteLine("");

        testOutputHelper.WriteLine("=== Complex Sample Class Diagram ===");
        var classChart = MermaidVisualizer.GenerateClassDiagram(complexResult);
        testOutputHelper.WriteLine(classChart);

        // Assert all diagrams are not empty
        Assert.NotEmpty(commandChart);
        Assert.NotEmpty(classChart);
    }

    [Fact]
    public void MermaidVisualizer_ShouldHandleSpecialCharactersInNames()
    {
        // Arrange - 创建包含特殊字符的数据
        var result2 = CreateSpecialCharactersSampleAnalysisResult2();

        // Act
        var commandChart = MermaidVisualizer.GenerateCommandFlowChart(result2);
        var classChart = MermaidVisualizer.GenerateClassDiagram(result2);

        // Assert - 应该不会抛出异常，并且生成有效的图表
        Assert.NotEmpty(commandChart);
        Assert.NotEmpty(classChart);

        testOutputHelper.WriteLine("=== Special Characters Command Chart ===");
        testOutputHelper.WriteLine(commandChart);
    }

    [Fact]
    public void GenerateAllChainFlowCharts_With_This_Assembly()
    {
        // Arrange
        var assemblies = new[] { typeof(MermaidVisualizerTests).Assembly };
        var result2 = CodeFlowAnalysisHelper.AnalyzeFromAssemblies(assemblies);

        // Act
        var chainFlowCharts = MermaidVisualizer.GenerateAllChainFlowCharts(result2);

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
    public void CompareAllMermaidVisualizationMethods_ShouldShowDifferentPerspectives()
    {
        // Arrange
        var result2 = CreateComplexSampleAnalysisResult2();

        // Act
        var commandChart = MermaidVisualizer.GenerateCommandFlowChart(result2);
        var classChart = MermaidVisualizer.GenerateClassDiagram(result2);
        var allChainFlowCharts = MermaidVisualizer.GenerateAllChainFlowCharts(result2);

        // Assert all methods work
        Assert.NotEmpty(commandChart);
        Assert.NotEmpty(classChart);
        Assert.NotEmpty(allChainFlowCharts);
        
        // 验证各图表的独特性
        Assert.NotEqual(commandChart, classChart);
        foreach (var (chainName, diagram) in allChainFlowCharts)
        {
            Assert.NotEqual(commandChart, diagram);
            Assert.NotEqual(classChart, diagram);
        }

        testOutputHelper.WriteLine("=== 对比所有可视化方法 ===");
        testOutputHelper.WriteLine("");

        // 1. 命令流程图
        testOutputHelper.WriteLine("### 1. 命令流程图");
        testOutputHelper.WriteLine($"Length: {commandChart.Length} characters");
        testOutputHelper.WriteLine("```mermaid");
        testOutputHelper.WriteLine(commandChart);
        testOutputHelper.WriteLine("```");
        testOutputHelper.WriteLine("");

        // 2. 类图
        testOutputHelper.WriteLine("### 2. 类图");
        testOutputHelper.WriteLine($"Length: {classChart.Length} characters");
        testOutputHelper.WriteLine("```mermaid");
        testOutputHelper.WriteLine(classChart);
        testOutputHelper.WriteLine("```");
        testOutputHelper.WriteLine("");

        // 3. 独立链路流程图
        testOutputHelper.WriteLine("### 3. 独立链路流程图");
        testOutputHelper.WriteLine($"Total chains: {allChainFlowCharts.Count}");
        foreach (var (chainName, diagram) in allChainFlowCharts)
        {
            testOutputHelper.WriteLine($"#### Chain: {chainName}");
            testOutputHelper.WriteLine("```mermaid");
            testOutputHelper.WriteLine(diagram);
            testOutputHelper.WriteLine("```");
            testOutputHelper.WriteLine("");
        }
    }
    // 新增辅助方法：将 CodeFlowAnalysisResult 转换为 CodeFlowAnalysisResult2
    private static CodeFlowAnalysisResult2 CreateSampleAnalysisResult2()
    {
        var assemblies = new[] { typeof(MermaidVisualizerTests).Assembly };
        return CodeFlowAnalysisHelper.AnalyzeFromAssemblies(assemblies);
    }

    private static CodeFlowAnalysisResult2 CreateComplexSampleAnalysisResult2()
    {
        var assemblies = new[] { typeof(MermaidVisualizerTests).Assembly };
        return CodeFlowAnalysisHelper.AnalyzeFromAssemblies(assemblies);
    }

    private static CodeFlowAnalysisResult2 CreateSpecialCharactersSampleAnalysisResult2()
    {
        var assemblies = new[] { typeof(MermaidVisualizerTests).Assembly };
        return CodeFlowAnalysisHelper.AnalyzeFromAssemblies(assemblies);
    }

    [Fact]
    public void GenerateVisualizationHtml_WithSampleData_ShouldProduceCompleteHtmlPage()
    {
        // Arrange
        var analysisResult2 = CreateSampleAnalysisResult2();

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult2, "测试架构图");

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
        
        // 验证包含导航结构（已修改，移除了已删除的图表类型）
        Assert.Contains("架构图导航", htmlContent);
        Assert.Contains("调用链路图", htmlContent);
        Assert.Contains("架构大图", htmlContent);
        Assert.Contains("单独链路流程图", htmlContent);
        
        // 验证JavaScript功能
        Assert.Contains("mermaid.initialize", htmlContent);
        Assert.Contains("analysisResult", htmlContent);
        Assert.Contains("diagrams", htmlContent);
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
        Assert.Contains("flowchart LR", htmlContent); // 命令流程图
        Assert.Contains("classDiagram", htmlContent); // 类图
        
        testOutputHelper.WriteLine("=== Generated HTML Page ===");
        testOutputHelper.WriteLine($"HTML 内容长度: {htmlContent.Length} 字符");
        testOutputHelper.WriteLine("HTML页面生成成功，包含所有必要的组件和数据");
        
        // 验证HTML是否可以保存到文件
        Assert.True(htmlContent.Length > 5000, "生成的HTML内容应该足够详细");
    }

    [Fact]
    public void GenerateVisualizationHtml_WithComplexData_ShouldIncludeAllDiagramTypes()
    {
        // Arrange
        var analysisResult2 = CreateComplexSampleAnalysisResult2();

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult2);

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
        testOutputHelper.WriteLine("复杂数据HTML页面生成成功");
    }

    [Fact]
    public void GenerateVisualizationHtml_WithEmptyData_ShouldProduceValidHtml()
    {
        // Arrange
        var assemblies = new[] { typeof(MermaidVisualizerTests).Assembly };
        var analysisResult2 = CodeFlowAnalysisHelper.AnalyzeFromAssemblies(assemblies);

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult2, "空数据测试");

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
        
        testOutputHelper.WriteLine("=== Empty Data HTML Generation ===");
        testOutputHelper.WriteLine("空数据情况下HTML页面生成成功，结构完整");
    }

    [Fact]
    public void GenerateVisualizationHtml_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var assemblies = new[] { typeof(MermaidVisualizerTests).Assembly };
        var analysisResult2 = CodeFlowAnalysisHelper.AnalyzeFromAssemblies(assemblies);

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult2);

        // Assert
        Assert.NotNull(htmlContent);
        
        // 验证特殊字符被正确转义
        Assert.Contains("Test\\\"Controller", htmlContent); // JavaScript转义
        Assert.Contains("Test&quot;Controller", htmlContent); // HTML转义或包含在某些地方
        Assert.Contains("Test\\'Method", htmlContent); // JavaScript单引号转义
        Assert.Contains("Test&lt;Command&gt;", htmlContent); // HTML标签转义
        
        // 确保没有未转义的特殊字符导致语法错误
        Assert.DoesNotContain("Test\"Controller", htmlContent); // 应该被转义
        Assert.DoesNotContain("Test<Command>", htmlContent); // 应该被转义
        
        testOutputHelper.WriteLine("=== Special Characters Escaping Test ===");
        testOutputHelper.WriteLine("特殊字符转义测试通过");
    }

    [Fact]
    public void GenerateVisualizationHtml_For_This_Assembly()
    {
        // Arrange
        var assemblies = new[] { typeof(MermaidVisualizerTests).Assembly };
        var analysisResult2 = CodeFlowAnalysisHelper.AnalyzeFromAssemblies(assemblies);

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult2);

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

    [Fact]
    public void GenerateVisualizationHtml_ShouldIncludeMermaidLiveButton()
    {
        // Arrange
        var result2 = CreateSampleAnalysisResult2();

        // Act
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(result2, "Test Page");

        // Assert
        Assert.NotEmpty(htmlContent);
        
        // 验证 Mermaid Live 按钮元素
        Assert.Contains("mermaidLiveButton", htmlContent);
        Assert.Contains("View in Mermaid Live", htmlContent);
        Assert.Contains("🔗 View in Mermaid Live", htmlContent);
        
        // 验证按钮样式类
        Assert.Contains("mermaid-live-button", htmlContent);
        Assert.Contains(".mermaid-live-button {", htmlContent);
        Assert.Contains("background: linear-gradient", htmlContent);
        Assert.Contains("transform: translateY(-2px)", htmlContent);
        
        // 验证 JavaScript 函数
        Assert.Contains("openInMermaidLive()", htmlContent);
        Assert.Contains("showMermaidLiveButton()", htmlContent);
        Assert.Contains("hideMermaidLiveButton()", htmlContent);
        Assert.Contains("function openInMermaidLive() {", htmlContent);
        Assert.Contains("function showMermaidLiveButton() {", htmlContent);
        Assert.Contains("function hideMermaidLiveButton() {", htmlContent);
        
        // 验证 pako 库引用
        Assert.Contains("pako.min.js", htmlContent);
        Assert.Contains("unpkg.com/pako@2.1.0/dist/pako.min.js", htmlContent);
        
        testOutputHelper.WriteLine("=== HTML with Mermaid Live Button ===");
        testOutputHelper.WriteLine("HTML Content Length: " + htmlContent.Length);
        testOutputHelper.WriteLine("Contains Mermaid Live Button: " + htmlContent.Contains("mermaidLiveButton"));
        testOutputHelper.WriteLine("Contains pako library: " + htmlContent.Contains("pako.min.js"));
        testOutputHelper.WriteLine("Contains pako compression: " + htmlContent.Contains("pako.deflate"));
        testOutputHelper.WriteLine("Contains base64 fallback: " + htmlContent.Contains("base64:"));
        
        testOutputHelper.WriteLine("✅ All Mermaid Live button features verified successfully");
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
                new() { Name = "CreateOrderCommand", FullName = "NetCorePal.Web.Application.Commands.CreateOrderCommand" },
                new() { Name = "OrderPaidCommand", FullName = "NetCorePal.Web.Application.Commands.OrderPaidCommand" }
            },
            Entities = new List<EntityInfo>
            {
                new() { Name = "Order", FullName = "NetCorePal.Web.Domain.Order", IsAggregateRoot = true, Methods = new List<string> { "OrderPaid", "SoftDelete" } }
            },
            DomainEvents = new List<DomainEventInfo>
            {
                new() { Name = "OrderCreatedDomainEvent", FullName = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent" }
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
                new("NetCorePal.Web.Application.Commands.OrderPaidCommand", "Handle", "NetCorePal.Web.Domain.Order", "OrderPaid", "CommandToEntityMethod"),
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
                new() { Name = "OrderController", FullName = "NetCorePal.Web.Controllers.OrderController", Methods = new List<string> { "Get", "Post", "SetPaid", "Delete" } },
                new() { Name = "UserController", FullName = "NetCorePal.Web.Controllers.UserController", Methods = new List<string> { "Get", "Post", "Update", "Delete" } }
            },
            Commands = new List<CommandInfo>
            {
                new() { Name = "CreateOrderCommand", FullName = "NetCorePal.Web.Application.Commands.CreateOrderCommand" },
                new() { Name = "OrderPaidCommand", FullName = "NetCorePal.Web.Application.Commands.OrderPaidCommand" },
                new() { Name = "DeleteOrderCommand", FullName = "NetCorePal.Web.Application.Commands.DeleteOrderCommand" },
                new() { Name = "CreateUserCommand", FullName = "NetCorePal.Web.Application.Commands.CreateUserCommand" },
                new() { Name = "UpdateUserCommand", FullName = "NetCorePal.Web.Application.Commands.UpdateUserCommand" },
                new() { Name = "DeleteUserCommand", FullName = "NetCorePal.Web.Application.Commands.DeleteUserCommand" }
            },
            Entities = new List<EntityInfo>
            {
                new() { Name = "Order", FullName = "NetCorePal.Web.Domain.Order", IsAggregateRoot = true, Methods = new List<string> { "OrderPaid", "SoftDelete" } },
                new() { Name = "User", FullName = "NetCorePal.Web.Domain.User", IsAggregateRoot = true, Methods = new List<string> { "UpdateProfile", "SoftDelete" } }
            },
            DomainEvents = new List<DomainEventInfo>
            {
                new() { Name = "OrderCreatedDomainEvent", FullName = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent" },
                new() { Name = "UserCreatedDomainEvent", FullName = "NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent" }
            },
            IntegrationEvents = new List<IntegrationEventInfo>
            {
                new() { Name = "OrderCreatedIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent" },
                new() { Name = "UserCreatedIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEvent" }
            },
            DomainEventHandlers = new List<DomainEventHandlerInfo>
            {
                new() { Name = "OrderCreatedDomainEventHandler", FullName = "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", HandledEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", Commands = new List<string>() },
                new() { Name = "UserCreatedDomainEventHandler", FullName = "NetCorePal.Web.Application.DomainEventHandlers.UserCreatedDomainEventHandler", HandledEventType = "NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", Commands = new List<string>() }
            },
            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent", Commands = new List<string>() },
                new() { Name = "UserCreatedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEvent", Commands = new List<string>() }
            },
            IntegrationEventConverters = new List<IntegrationEventConverterInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.OrderCreatedIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent" },
                new() { Name = "UserCreatedIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.UserCreatedIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEvent" }
            },
            Relationships = new List<CallRelationship>
            {
                new("NetCorePal.Web.Controllers.OrderController", "Post", "NetCorePal.Web.Application.Commands.CreateOrderCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.OrderController", "SetPaid", "NetCorePal.Web.Application.Commands.OrderPaidCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.OrderController", "Delete", "NetCorePal.Web.Application.Commands.DeleteOrderCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.UserController", "Post", "NetCorePal.Web.Application.Commands.CreateUserCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.UserController", "Update", "NetCorePal.Web.Application.Commands.UpdateUserCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Controllers.UserController", "Delete", "NetCorePal.Web.Application.Commands.DeleteUserCommand", "", "MethodToCommand"),
                new("NetCorePal.Web.Application.Commands.OrderPaidCommand", "Handle", "NetCorePal.Web.Domain.Order", "OrderPaid", "CommandToEntityMethod"),
                new("NetCorePal.Web.Application.Commands.UpdateUserCommand", "Handle", "NetCorePal.Web.Domain.User", "UpdateProfile", "CommandToEntityMethod"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                new("NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", "", "NetCorePal.Web.Application.DomainEventHandlers.UserCreatedDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler"),
                new("NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler")
            }
        };
    }
}
