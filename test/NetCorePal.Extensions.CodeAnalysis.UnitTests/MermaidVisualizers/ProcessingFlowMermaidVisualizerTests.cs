using System.Collections.Generic;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;
using Xunit;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.MermaidVisualizers;

public class ProcessingFlowMermaidVisualizerTests
{
    [Fact]
    public void GenerateAllChainFlowCharts_ShouldReturnChainDiagrams()
    {
        var nodes = new List<Node>
        {
            new Node { Id = "OrderController.Create", Name = "OrderController.Create", FullName = "Test.Controllers.OrderController.Create", Type = NodeType.ControllerMethod },
            new Node { Id = "CreateOrderCommand", Name = "CreateOrderCommand", FullName = "Test.Application.Commands.CreateOrderCommand", Type = NodeType.Command },
            new Node { Id = "Order.Create", Name = "Order.Create", FullName = "Test.Domain.Order.Create", Type = NodeType.EntityMethod },
            new Node { Id = "OrderCreatedDomainEvent", Name = "OrderCreatedDomainEvent", FullName = "Test.Domain.DomainEvents.OrderCreatedDomainEvent", Type = NodeType.DomainEvent },
        };
        var relationships = new List<Relationship>
        {
            new Relationship(nodes[0], nodes[1], RelationshipType.ControllerMethodToCommand),
            new Relationship(nodes[1], nodes[2], RelationshipType.CommandToEntityMethod),
            new Relationship(nodes[2], nodes[3], RelationshipType.EntityMethodToDomainEvent),
        };
        var result2 = new CodeFlowAnalysisResult { Nodes = nodes, Relationships = relationships };

        var diagrams = ProcessingFlowMermaidVisualizer.GenerateMermaid(result2);
        Assert.Single(diagrams);
        Assert.Contains("OrderController.Create", diagrams[0].Diagram);
        Assert.Contains("CreateOrderCommand", diagrams[0].Diagram);
        Assert.Contains("Order.Create", diagrams[0].Diagram);
        Assert.Contains("OrderCreatedDomainEvent", diagrams[0].Diagram);
        Assert.Contains("flowchart TD", diagrams[0].Diagram);
    }
}
