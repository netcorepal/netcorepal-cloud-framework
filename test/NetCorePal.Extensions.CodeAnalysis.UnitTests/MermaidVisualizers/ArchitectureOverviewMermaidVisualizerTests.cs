using System.Collections.Generic;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;
using Xunit;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.MermaidVisualizers;

public class ArchitectureOverviewMermaidVisualizerTests
{
    [Fact]
    public void GenerateClassDiagram_ShouldIncludeNodesAndRelationships()
    {
        var nodes = new List<Node>
        {
            new Node { Id = "Order", Name = "Order", FullName = "Test.Domain.Order", Type = NodeType.Aggregate, Properties = new Dictionary<string, object> { ["IsAggregateRoot"] = true } },
            new Node { Id = "CreateOrderCommand", Name = "CreateOrderCommand", FullName = "Test.Application.Commands.CreateOrderCommand", Type = NodeType.Command },
        };
        var relationships = new List<Relationship>
        {
            new Relationship(nodes[1], nodes[0], RelationshipType.CommandToEntityMethod),
        };
        var result2 = new CodeFlowAnalysisResult { Nodes = nodes, Relationships = relationships };

        var diagram = ArchitectureOverviewMermaidVisualizer.GenerateMermaid(result2);
        Assert.Contains("Order", diagram);
        Assert.Contains("CreateOrderCommand", diagram);
        Assert.Contains("flowchart LR", diagram);
    }
}
