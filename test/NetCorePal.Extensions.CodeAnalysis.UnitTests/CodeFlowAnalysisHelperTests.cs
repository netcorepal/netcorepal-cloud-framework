using System.Reflection;
using Xunit;
using NetCorePal.Extensions.CodeAnalysis;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class CodeFlowAnalysisHelperTests
{
    [Fact]
    public void AnalyzeFromAssemblies_ShouldReturnNodesAndRelationships()
    {
        // 使用当前测试程序集进行分析
        var assembly = Assembly.GetExecutingAssembly();
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(assembly);

        Assert.NotNull(result);
        Assert.NotNull(result.Nodes);
        Assert.NotNull(result.Relationships);

        Assert.Equal(2, result.Nodes.Count(n => n.Type == NodeType.Controller));
        Assert.Equal(6, result.Nodes.Count(n => n.Type == NodeType.ControllerMethod));
        Assert.Equal(5, result.Nodes.Count(n => n.Type == NodeType.Endpoint));
        Assert.Equal(1, result.Nodes.Count(n => n.Type == NodeType.CommandSender));
        Assert.Equal(3, result.Nodes.Count(n => n.Type == NodeType.CommandSenderMethod));
        Assert.Equal(10, result.Nodes.Count(n => n.Type == NodeType.Command));
        Assert.Equal(2, result.Nodes.Count(n => n.Type == NodeType.CommandHandler));
        Assert.Equal(1, result.Nodes.Count(n => n.Type == NodeType.Aggregate));
        Assert.Equal(3, result.Nodes.Count(n => n.Type == NodeType.AggregateMethod));
        Assert.Equal(3, result.Nodes.Count(n => n.Type == NodeType.DomainEvent));
        Assert.Equal(3, result.Nodes.Count(n => n.Type == NodeType.IntegrationEvent));
        Assert.Equal(2, result.Nodes.Count(n => n.Type == NodeType.DomainEventHandler));
        Assert.Equal(4, result.Nodes.Count(n => n.Type == NodeType.IntegrationEventHandler));
        Assert.Equal(3, result.Nodes.Count(n => n.Type == NodeType.IntegrationEventConverter));
        Assert.Equal(5, result.Relationships.Count(r => r.Type == RelationshipType.ControllerToCommand));
        Assert.Equal(60, result.Relationships.Count(r => r.Type == RelationshipType.ControllerMethodToCommand));
        Assert.Equal(4, result.Relationships.Count(r => r.Type == RelationshipType.EndpointToCommand));
        Assert.Equal(10, result.Relationships.Count(r => r.Type == RelationshipType.CommandSenderToCommand));
        Assert.Equal(30, result.Relationships.Count(r => r.Type == RelationshipType.CommandSenderMethodToCommand));
        Assert.Equal(1, result.Relationships.Count(r => r.Type == RelationshipType.CommandToAggregate));
        Assert.Equal(30, result.Relationships.Count(r => r.Type == RelationshipType.CommandToAggregateMethod));
        Assert.Equal(9, result.Relationships.Count(r => r.Type == RelationshipType.AggregateMethodToDomainEvent));
        Assert.Equal(0, result.Relationships.Count(r => r.Type == RelationshipType.EntityMethodToDomainEvent));
        Assert.Equal(6, result.Relationships.Count(r => r.Type == RelationshipType.DomainEventToHandler));
        Assert.Equal(12, result.Relationships.Count(r => r.Type == RelationshipType.IntegrationEventToHandler));
        Assert.Equal(9, result.Relationships.Count(r => r.Type == RelationshipType.DomainEventToIntegrationEvent));
    }
}