using System.Collections.Generic;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;
using Xunit;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.MermaidVisualizers;

public class AggregateRelationMermaidVisualizerTests
{
    [Fact]
    public void GenerateAllAggregateRelationDiagrams_ShouldReturnDiagramsForAggregates()
    {
        // 构造测试数据
        var nodes = new List<Node>
        {
            new Node { Id = "Order", Name = "Order", FullName = "Test.Domain.Order", Type = NodeType.Aggregate, Properties = new Dictionary<string, object> { ["IsAggregateRoot"] = true } },
            new Node { Id = "DeliverRecord", Name = "DeliverRecord", FullName = "Test.Domain.DeliverRecord", Type = NodeType.Aggregate, Properties = new Dictionary<string, object> { ["IsAggregateRoot"] = true } },
            new Node { Id = "CreateOrderCommand", Name = "CreateOrderCommand", FullName = "Test.Application.Commands.CreateOrderCommand", Type = NodeType.Command },
            new Node { Id = "OrderCreatedDomainEvent", Name = "OrderCreatedDomainEvent", FullName = "Test.Domain.DomainEvents.OrderCreatedDomainEvent", Type = NodeType.DomainEvent },
        };
        var relationships = new List<Relationship>
        {
            new Relationship { FromNode = nodes[2], ToNode = nodes[0], Type = RelationshipType.CommandToAggregateMethod },
            new Relationship { FromNode = nodes[0], ToNode = nodes[3], Type = RelationshipType.EntityMethodToDomainEvent },
        };
        var result2 = new CodeFlowAnalysisResult2 { Nodes = nodes, Relationships = relationships };

        var diagrams = AggregateRelationMermaidVisualizer.GenerateAllAggregateRelationDiagrams(result2);
        Assert.Equal(2, diagrams.Count);
        Assert.Contains(diagrams, d => d.AggregateName == "Order");
        Assert.Contains(diagrams, d => d.AggregateName == "DeliverRecord");
        Assert.All(diagrams, d => Assert.Contains("flowchart LR", d.Diagram));
    }

    [Fact]
    public void GenerateAggregateRelationDiagram_ShouldIncludeAggregateAndRelatedNodes()
    {
        var nodes = new List<Node>
        {
            new Node { Id = "Order", Name = "Order", FullName = "Test.Domain.Order", Type = NodeType.Aggregate, Properties = new Dictionary<string, object> { ["IsAggregateRoot"] = true } },
            new Node { Id = "CreateOrderCommand", Name = "CreateOrderCommand", FullName = "Test.Application.Commands.CreateOrderCommand", Type = NodeType.Command },
            new Node { Id = "OrderCreatedDomainEvent", Name = "OrderCreatedDomainEvent", FullName = "Test.Domain.DomainEvents.OrderCreatedDomainEvent", Type = NodeType.DomainEvent },
        };
        var relationships = new List<Relationship>
        {
            new Relationship { FromNode = nodes[1], ToNode = nodes[0], Type = RelationshipType.CommandToAggregateMethod },
            new Relationship { FromNode = nodes[0], ToNode = nodes[2], Type = RelationshipType.EntityMethodToDomainEvent },
        };
        var result2 = new CodeFlowAnalysisResult2 { Nodes = nodes, Relationships = relationships };

        var diagram = AggregateRelationMermaidVisualizer.GenerateAggregateRelationDiagram(result2, "Test.Domain.Order");
        Assert.Contains("Order", diagram);
        Assert.Contains("CreateOrderCommand", diagram);
        Assert.Contains("OrderCreatedDomainEvent", diagram);
        Assert.Contains("flowchart LR", diagram);
    }
}
