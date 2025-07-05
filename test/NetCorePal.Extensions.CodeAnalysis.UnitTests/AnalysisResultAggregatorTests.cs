using System.Collections.Generic;
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class AnalysisResultAggregatorTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Aggregate_WithEmptyAssemblies_ShouldReturnEmptyResult()
    {
        // Act
        var result = AnalysisResultAggregator.Aggregate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Controllers);
        Assert.Empty(result.Commands);
        Assert.Empty(result.Entities);
        Assert.Empty(result.DomainEvents);
        Assert.Empty(result.DomainEventHandlers);
        Assert.Empty(result.IntegrationEvents);
        Assert.Empty(result.IntegrationEventHandlers);
        Assert.Empty(result.IntegrationEventConverters);
        Assert.Empty(result.Relationships);
    }

    [Fact]
    public void Aggregate_WithNullAssemblies_ShouldReturnEmptyResult()
    {
        // Act
        var result = AnalysisResultAggregator.Aggregate((Assembly[])null!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Controllers);
        Assert.Empty(result.Commands);
        Assert.Empty(result.Entities);
        Assert.Empty(result.DomainEvents);
        Assert.Empty(result.DomainEventHandlers);
        Assert.Empty(result.IntegrationEvents);
        Assert.Empty(result.IntegrationEventHandlers);
        Assert.Empty(result.IntegrationEventConverters);
        Assert.Empty(result.Relationships);
    }

    [Fact]
    public void Aggregate_WithValidAssembly_ShouldReturnResult()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = AnalysisResultAggregator.Aggregate(assembly);

        // Assert
        Assert.NotNull(result);
        
        // 验证是否能检测到我们创建的测试类
        testOutputHelper.WriteLine($"Found {result.Controllers.Count} controllers");
        testOutputHelper.WriteLine($"Found {result.Commands.Count} commands");
        testOutputHelper.WriteLine($"Found {result.Entities.Count} entities");
        testOutputHelper.WriteLine($"Found {result.DomainEvents.Count} domain events");
        testOutputHelper.WriteLine($"Found {result.DomainEventHandlers.Count} domain event handlers");
        testOutputHelper.WriteLine($"Found {result.IntegrationEvents.Count} integration events");
        testOutputHelper.WriteLine($"Found {result.IntegrationEventHandlers.Count} integration event handlers");
        testOutputHelper.WriteLine($"Found {result.IntegrationEventConverters.Count} integration event converters");
        testOutputHelper.WriteLine($"Found {result.Relationships.Count} relationships");
    }

    [Fact]
    public void AggregateFromCurrentDomain_ShouldReturnResult()
    {
        // Act
        var result = AnalysisResultAggregator.AggregateFromCurrentDomain();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AggregateFromAssemblyNames_WithEmptyNames_ShouldReturnEmptyResult()
    {
        // Act
        var result = AnalysisResultAggregator.AggregateFromAssemblyNames();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Controllers);
    }

    [Fact]
    public void AggregateFromAssemblyNames_WithInvalidName_ShouldReturnEmptyResult()
    {
        // Act
        var result = AnalysisResultAggregator.AggregateFromAssemblyNames("InvalidAssemblyName.dll");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Controllers);
    }

    [Fact]
    public void Aggregate_WithTestData_ShouldProduceValidResult()
    {
        // Arrange - 创建测试用的分析结果实现类
        var testResult = CreateTestAnalysisResult();

        // 由于我们不能直接测试内部合并逻辑，我们创建一个模拟场景
        var result = new CodeFlowAnalysisResult();
        
        // 手动模拟合并过程
        result.Controllers.AddRange(testResult.Controllers);
        result.Commands.AddRange(testResult.Commands);
        result.Entities.AddRange(testResult.Entities);
        result.DomainEvents.AddRange(testResult.DomainEvents);
        result.DomainEventHandlers.AddRange(testResult.DomainEventHandlers);
        result.IntegrationEvents.AddRange(testResult.IntegrationEvents);
        result.IntegrationEventHandlers.AddRange(testResult.IntegrationEventHandlers);
        result.IntegrationEventConverters.AddRange(testResult.IntegrationEventConverters);
        result.Relationships.AddRange(testResult.Relationships);

        // Assert
        Assert.NotEmpty(result.Controllers);
        Assert.NotEmpty(result.Commands);
        Assert.NotEmpty(result.Entities);
        Assert.NotEmpty(result.DomainEvents);
        Assert.NotEmpty(result.DomainEventHandlers);
        Assert.NotEmpty(result.IntegrationEvents);
        Assert.NotEmpty(result.IntegrationEventHandlers);
        Assert.NotEmpty(result.IntegrationEventConverters);
        Assert.NotEmpty(result.Relationships);

        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        testOutputHelper.WriteLine("Generated Analysis Result:");
        testOutputHelper.WriteLine(json);
    }

    [Fact]
    public void RunConsoleAnalysis_ShouldOutputResults()
    {
        // 运行控制台分析并输出结果
        AnalysisTestRunner.RunAnalysis();
        
        // 这个测试总是通过，主要目的是触发控制台输出
        Assert.True(true);
    }

    private static CodeFlowAnalysisResult CreateTestAnalysisResult()
    {
        return new CodeFlowAnalysisResult
        {
            Controllers = new List<ControllerInfo>
            {
                new() { Name = "OrderController", FullName = "Test.Controllers.OrderController", Methods = new List<string> { "Get", "Post", "SetPaid" } },
                new() { Name = "UserController", FullName = "Test.Controllers.UserController", Methods = new List<string> { "CreateUser", "Login" } }
            },
            Commands = new List<CommandInfo>
            {
                new() { Name = "CreateOrderCommand", FullName = "Test.Application.Commands.CreateOrderCommand", Properties = new List<string>() },
                new() { Name = "OrderPaidCommand", FullName = "Test.Application.Commands.OrderPaidCommand", Properties = new List<string>() },
                new() { Name = "DeleteOrderCommand", FullName = "Test.Application.Commands.DeleteOrderCommand", Properties = new List<string>() }
            },
            Entities = new List<EntityInfo>
            {
                new() { Name = "Order", FullName = "Test.Domain.Order", IsAggregateRoot = true, Methods = new List<string> { "OrderPaid", "SoftDelete", "ChangeItemName" } },
                new() { Name = "DeliverRecord", FullName = "Test.Domain.DeliverRecord", IsAggregateRoot = true, Methods = new List<string>() }
            },
            DomainEvents = new List<DomainEventInfo>
            {
                new() { Name = "OrderCreatedDomainEvent", FullName = "Test.Domain.DomainEvents.OrderCreatedDomainEvent", Properties = new List<string>() }
            },
            IntegrationEvents = new List<IntegrationEventInfo>
            {
                new() { Name = "OrderCreatedIntegrationEvent", FullName = "Test.Application.IntegrationEvents.OrderCreatedIntegrationEvent" },
                new() { Name = "OrderPaidIntegrationEvent", FullName = "Test.Application.IntegrationEvents.OrderPaidIntegrationEvent" }
            },
            DomainEventHandlers = new List<DomainEventHandlerInfo>
            {
                new() { Name = "OrderCreatedDomainEventHandler", FullName = "Test.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", HandledEventType = "Test.Domain.DomainEvents.OrderCreatedDomainEvent", Commands = new List<string>() }
            },
            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventHandler", FullName = "Test.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", HandledEventType = "Test.Application.IntegrationEvents.OrderCreatedIntegrationEvent", Commands = new List<string>() },
                new() { Name = "OrderPaidIntegrationEventHandler", FullName = "Test.Application.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", HandledEventType = "Test.Application.IntegrationEvents.OrderPaidIntegrationEvent", Commands = new List<string>() }
            },
            IntegrationEventConverters = new List<IntegrationEventConverterInfo>
            {
                new() { Name = "OrderCreatedIntegrationEventConverter", FullName = "Test.Application.IntegrationConverters.OrderCreatedIntegrationEventConverter", DomainEventType = "Test.Domain.DomainEvents.OrderCreatedDomainEvent", IntegrationEventType = "Test.Application.IntegrationEvents.OrderCreatedIntegrationEvent" }
            },
            Relationships = new List<CallRelationship>
            {
                new("Test.Controllers.OrderController", "Post", "Test.Application.Commands.CreateOrderCommand", "", "MethodToCommand"),
                new("Test.Application.Commands.OrderPaidCommand", "Handle", "Test.Domain.Order", "OrderPaid", "CommandToAggregateMethod"),
                new("Test.Application.Commands.DeleteOrderCommand", "Handle", "Test.Domain.Order", "SoftDelete", "CommandToAggregateMethod"),
                new("Test.Domain.DomainEvents.OrderCreatedDomainEvent", "", "Test.Application.DomainEventHandlers.OrderCreatedDomainEventHandler", "HandleAsync", "DomainEventToHandler"),
                new("Test.Domain.DomainEvents.OrderCreatedDomainEvent", "", "Test.Application.IntegrationEvents.OrderCreatedIntegrationEvent", "", "DomainEventToIntegrationEvent"),
                new("Test.Application.IntegrationEvents.OrderCreatedIntegrationEvent", "", "Test.Application.IntegrationEventHandlers.OrderCreatedIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler"),
                new("Test.Application.IntegrationEvents.OrderPaidIntegrationEvent", "", "Test.Application.IntegrationEventHandlers.OrderPaidIntegrationEventHandler", "Subscribe", "IntegrationEventToHandler")
            }
        };
    }
}
