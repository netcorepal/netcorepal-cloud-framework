using System.Collections.Generic;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;
using Xunit;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.MermaidVisualizers;

public class ChainFlowMermaidVisualizerTests
{
    [Fact]
    public void GenerateAllChainFlowCharts_ShouldReturnChainDiagrams()
    {
        var nodes = new List<Node>
        {
            new Node { Id = "Order", Name = "Order", FullName = "Test.Domain.Order", Type = NodeType.Aggregate },
            new Node { Id = "CreateOrderCommand", Name = "CreateOrderCommand", FullName = "Test.Application.Commands.CreateOrderCommand", Type = NodeType.Command },
            new Node { Id = "OrderCreatedDomainEvent", Name = "OrderCreatedDomainEvent", FullName = "Test.Domain.DomainEvents.OrderCreatedDomainEvent", Type = NodeType.DomainEvent },
        };
        var relationships = new List<Relationship>
        {
            new Relationship(nodes[1], nodes[0], RelationshipType.CommandToAggregateMethod),
            new Relationship(nodes[0], nodes[2], RelationshipType.EntityMethodToDomainEvent),
        };
        var result2 = new CodeFlowAnalysisResult { Nodes = nodes, Relationships = relationships };

        var diagrams = ChainFlowMermaidVisualizer.GenerateAllChainFlowCharts(result2);
        Assert.Single(diagrams);
        Assert.Contains("CreateOrderCommand", diagrams[0].Diagram);
        Assert.Contains("Order", diagrams[0].Diagram);
        Assert.Contains("OrderCreatedDomainEvent", diagrams[0].Diagram);
        Assert.Contains("flowchart TD", diagrams[0].Diagram);
    }
}
