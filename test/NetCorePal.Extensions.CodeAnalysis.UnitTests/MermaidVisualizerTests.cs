using System.Collections.Generic;
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
                new() { Name = "SetOrderItemNameCommand", FullName = "NetCorePal.Web.Application.Commands.SetOrderItemNameCommand", Properties = new List<string>() },
                new() { Name = "CreateUserCommand", FullName = "NetCorePal.Web.Application.Commands.CreateUserCommand", Properties = new List<string>() }
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
                new() { Name = "UserCreatedIntegrationEvent", FullName = "NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent" }
            },
            DomainEventHandlers = new List<DomainEventHandlerInfo>
            {
                new() { Name = "OrderCreatedDomainEventHandler", FullName = "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", HandledEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", Commands = new List<string>() },
                new() { Name = "OrderPaidDomainEventHandler", FullName = "NetCorePal.Web.Application.DomainEventHandlers.OrderPaidDomainEventHandler", HandledEventType = "NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", Commands = new List<string>() }
            },
            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.SendNotificationCommand" } },
                new() { Name = "OrderPaidIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderPaidIntegrationEvent", Commands = new List<string> { "NetCorePal.Web.Application.Commands.UpdateInventoryCommand" } },
                new() { Name = "UserCreatedIntegrationEventHandler", FullName = "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEventHandler", HandledEventType = "NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent", Commands = new List<string>() }
            },
            IntegrationEventConverters = new List<IntegrationEventConverterInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventConverter", FullName = "NetCorePal.Web.Application.IntegrationConverters.OrderCreatedIntegrationEventConverter", DomainEventType = "NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", IntegrationEventType = "NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent" },
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
                new("NetCorePal.Web.Application.Commands.OrderPaidCommand", "Handle", "NetCorePal.Web.Domain.Order", "OrderPaid", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Application.Commands.DeleteOrderCommand", "Handle", "NetCorePal.Web.Domain.Order", "SoftDelete", "CommandToAggregateMethod"),
                new("NetCorePal.Web.Application.Commands.SetOrderItemNameCommand", "Handle", "NetCorePal.Web.Domain.Order", "ChangeItemName", "CommandToAggregateMethod"),
                
                // Domain Event to Handler relationships
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                new("NetCorePal.Web.Domain.DomainEvents.OrderPaidDomainEvent", "", "NetCorePal.Web.Application.DomainEventHandlers.OrderPaidDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                
                // Domain Event to Integration Event relationships
                new("NetCorePal.Web.Domain.DomainEvents.OrderCreatedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("NetCorePal.Web.Domain.DomainEvents.UserCreatedDomainEvent", "", "NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                
                // Integration Event to Handler relationships
                new("NetCorePal.Web.Application.IntegrationEvents.OrderCreatedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler"),
                new("NetCorePal.Web.Application.IntegrationEvents.OrderPaidIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler"),
                new("NetCorePal.Web.Application.IntegrationEvents.UserCreatedIntegrationEvent", "", "NetCorePal.Web.Application.IntegrationEventHandlers.UserCreatedIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler")
            }
        };
    }
}
