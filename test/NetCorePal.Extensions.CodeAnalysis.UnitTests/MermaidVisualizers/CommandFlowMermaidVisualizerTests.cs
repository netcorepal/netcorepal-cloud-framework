using System.Collections.Generic;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;
using Xunit;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.MermaidVisualizers;

public class CommandFlowMermaidVisualizerTests
{
    [Fact]
    public void GenerateCommandFlowChart_ShouldIncludeCommandAndEntity()
    {
        var nodes = new List<Node>
        {
            new Node { Id = "Order", Name = "Order", FullName = "Test.Domain.Order", Type = NodeType.Entity },
            new Node { Id = "CreateOrderCommand", Name = "CreateOrderCommand", FullName = "Test.Application.Commands.CreateOrderCommand", Type = NodeType.Command },
        };
        var relationships = new List<Relationship>
        {
            new Relationship { FromNode = nodes[1], ToNode = nodes[0], Type = RelationshipType.CommandToEntityMethod },
        };
        var result2 = new CodeFlowAnalysisResult2 { Nodes = nodes, Relationships = relationships };

        var diagram = CommandFlowMermaidVisualizer.GenerateCommandFlowChart(result2);
        Assert.Contains("Order", diagram);
        Assert.Contains("CreateOrderCommand", diagram);
        Assert.Contains("flowchart LR", diagram);
    }
}
