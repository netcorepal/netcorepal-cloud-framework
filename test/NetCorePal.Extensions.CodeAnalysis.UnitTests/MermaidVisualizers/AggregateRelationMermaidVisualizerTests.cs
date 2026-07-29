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
            new Relationship(nodes[2], nodes[0], RelationshipType.CommandToEntityMethod),
            new Relationship(nodes[0], nodes[3], RelationshipType.EntityMethodToDomainEvent),
        };
        var result2 = new CodeFlowAnalysisResult { Nodes = nodes, Relationships = relationships };

        var diagrams = AggregateRelationMermaidVisualizer.GenerateAllAggregateMermaid(result2);
        Assert.Equal(2, diagrams.Count);
        Assert.Contains(diagrams, d => d.AggregateName == "Order");
        Assert.Contains(diagrams, d => d.AggregateName == "DeliverRecord");
        Assert.All(diagrams, d => Assert.Contains("flowchart TD", d.Diagram));
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
            new Relationship(nodes[1], nodes[0], RelationshipType.CommandToEntityMethod),
            new Relationship(nodes[0], nodes[2], RelationshipType.EntityMethodToDomainEvent),
        };
        var result2 = new CodeFlowAnalysisResult { Nodes = nodes, Relationships = relationships };

        var diagram = AggregateRelationMermaidVisualizer.GenerateMermaid(result2, "Test.Domain.Order");
        Assert.Contains("Order", diagram);
        Assert.Contains("CreateOrderCommand", diagram);
        Assert.Contains("OrderCreatedDomainEvent", diagram);
        Assert.Contains("flowchart TD", diagram);
    }

    [Fact]
    public void GenerateAggregateRelationDiagram_MultipleEndpoints_ShouldNotMergeEndpointNodes()
    {
        // Reproduces bug: multiple endpoints (each with a same-named method e.g. HandleAsync)
        // were all assigned the same Mermaid node ID, causing all commands to appear connected
        // to one endpoint node.
        var deptAggregate = new Node
        {
            Id = "Dept", Name = "Dept",
            FullName = "Test.Domain.Dept",
            Type = NodeType.Aggregate,
            Properties = new Dictionary<string, object> { ["IsAggregateRoot"] = true }
        };
        var createCommand = new Node
        {
            Id = "CreateDeptCommand", Name = "CreateDeptCommand",
            FullName = "Test.Application.Commands.CreateDeptCommand",
            Type = NodeType.Command
        };
        var updateCommand = new Node
        {
            Id = "UpdateDeptCommand", Name = "UpdateDeptCommand",
            FullName = "Test.Application.Commands.UpdateDeptCommand",
            Type = NodeType.Command
        };
        var deleteCommand = new Node
        {
            Id = "DeleteDeptCommand", Name = "DeleteDeptCommand",
            FullName = "Test.Application.Commands.DeleteDeptCommand",
            Type = NodeType.Command
        };
        // Three separate endpoints, each with FullName ending in ".HandleAsync"
        var createEndpoint = new Node
        {
            Id = "Test.Endpoints.CreateDeptEndpoint.HandleAsync",
            Name = "CreateDeptEndpoint",
            FullName = "Test.Endpoints.CreateDeptEndpoint.HandleAsync",
            Type = NodeType.Endpoint
        };
        var updateEndpoint = new Node
        {
            Id = "Test.Endpoints.UpdateDeptEndpoint.HandleAsync",
            Name = "UpdateDeptEndpoint",
            FullName = "Test.Endpoints.UpdateDeptEndpoint.HandleAsync",
            Type = NodeType.Endpoint
        };
        var deleteEndpoint = new Node
        {
            Id = "Test.Endpoints.DeleteDeptEndpoint.HandleAsync",
            Name = "DeleteDeptEndpoint",
            FullName = "Test.Endpoints.DeleteDeptEndpoint.HandleAsync",
            Type = NodeType.Endpoint
        };

        var nodes = new List<Node>
        {
            deptAggregate, createCommand, updateCommand, deleteCommand,
            createEndpoint, updateEndpoint, deleteEndpoint
        };
        var relationships = new List<Relationship>
        {
            new Relationship(createEndpoint, createCommand, RelationshipType.EndpointToCommand),
            new Relationship(updateEndpoint, updateCommand, RelationshipType.EndpointToCommand),
            new Relationship(deleteEndpoint, deleteCommand, RelationshipType.EndpointToCommand),
            new Relationship(createCommand, deptAggregate, RelationshipType.CommandToEntityMethod),
            new Relationship(updateCommand, deptAggregate, RelationshipType.CommandToEntityMethod),
            new Relationship(deleteCommand, deptAggregate, RelationshipType.CommandToEntityMethod),
        };
        var result = new CodeFlowAnalysisResult { Nodes = nodes, Relationships = relationships };

        var diagram = AggregateRelationMermaidVisualizer.GenerateMermaid(result, "Test.Domain.Dept");

        // All three endpoints must appear with their own unique node IDs in the diagram
        Assert.Contains("CreateDeptEndpoint", diagram);
        Assert.Contains("UpdateDeptEndpoint", diagram);
        Assert.Contains("DeleteDeptEndpoint", diagram);

        // Each endpoint must be connected only to its own command.
        // The unique node IDs (based on FullName) must be distinct in the diagram.
        Assert.Contains("Test_Endpoints_CreateDeptEndpoint_HandleAsync", diagram);
        Assert.Contains("Test_Endpoints_UpdateDeptEndpoint_HandleAsync", diagram);
        Assert.Contains("Test_Endpoints_DeleteDeptEndpoint_HandleAsync", diagram);
    }
}
