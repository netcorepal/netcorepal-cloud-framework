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
        Assert.Equal(5, result.Nodes.Count(n => n.Type == NodeType.Aggregate));
        Assert.Equal(7, result.Nodes.Count(n => n.Type == NodeType.EntityMethod));
        Assert.Equal(7, result.Nodes.Count(n => n.Type == NodeType.DomainEvent));
        Assert.Equal(3, result.Nodes.Count(n => n.Type == NodeType.IntegrationEvent));
        Assert.Equal(2, result.Nodes.Count(n => n.Type == NodeType.DomainEventHandler));
        Assert.Equal(4, result.Nodes.Count(n => n.Type == NodeType.IntegrationEventHandler));
        Assert.Equal(3, result.Nodes.Count(n => n.Type == NodeType.IntegrationEventConverter));
        Assert.Equal(5, result.Relationships.Count(r => r.Type == RelationshipType.ControllerToCommand));
        Assert.Equal(6, result.Relationships.Count(r => r.Type == RelationshipType.ControllerMethodToCommand));
        Assert.Equal(4, result.Relationships.Count(r => r.Type == RelationshipType.EndpointToCommand));
        Assert.Equal(2, result.Relationships.Count(r => r.Type == RelationshipType.CommandSenderToCommand));
        Assert.Equal(3, result.Relationships.Count(r => r.Type == RelationshipType.CommandSenderMethodToCommand));
        Assert.Equal(1, result.Relationships.Count(r => r.Type == RelationshipType.CommandToAggregate));
        Assert.Equal(1, result.Relationships.Count(r => r.Type == RelationshipType.CommandToEntityMethod));
        Assert.Equal(13, result.Relationships.Count(r => r.Type == RelationshipType.AggregateToDomainEvent));
        Assert.Equal(2, result.Relationships.Count(r => r.Type == RelationshipType.EntityMethodToEntityMethod));
        Assert.Equal(7, result.Relationships.Count(r => r.Type == RelationshipType.EntityMethodToDomainEvent));
        Assert.Equal(2, result.Relationships.Count(r => r.Type == RelationshipType.DomainEventToHandler));
        Assert.Equal(2, result.Relationships.Count(r => r.Type == RelationshipType.DomainEventHandlerToCommand));
        Assert.Equal(4, result.Relationships.Count(r => r.Type == RelationshipType.IntegrationEventToHandler));
        Assert.Equal(3, result.Relationships.Count(r => r.Type == RelationshipType.DomainEventToIntegrationEvent));

        // 验证节点数量
        Assert.Equal(60, result.Nodes.Count);
        Assert.Equal(59, result.Relationships.Count);
    }

    [Fact]
    public void AnalyzeFromAssemblies_WithRecursiveEntities_ShouldNotCauseStackOverflow()
    {
        // 这个测试验证递归实体不会导致栈溢出异常
        // 我们添加了以下递归场景的测试类：
        // 1. RecursiveAggregateA <-> RecursiveAggregateB (双向循环)
        // 2. RecursiveAggregateA -> RecursiveAggregateC -> RecursiveAggregateA (三角循环)
        // 3. SelfReferencingAggregate -> SelfReferencingAggregate (自引用)
        
        var assembly = Assembly.GetExecutingAssembly();
        
        // 如果递归检测有问题，这里会抛出 StackOverflowException
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(assembly);

        Assert.NotNull(result);
        Assert.NotNull(result.Nodes);
        Assert.NotNull(result.Relationships);

        // 验证递归实体被正确识别为聚合根
        var recursiveAggregateA = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.Aggregate && 
            n.FullName?.Contains("RecursiveAggregateA") == true);
        Assert.NotNull(recursiveAggregateA);

        var recursiveAggregateB = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.Aggregate && 
            n.FullName?.Contains("RecursiveAggregateB") == true);
        Assert.NotNull(recursiveAggregateB);

        var recursiveAggregateC = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.Aggregate && 
            n.FullName?.Contains("RecursiveAggregateC") == true);
        Assert.NotNull(recursiveAggregateC);

        var selfReferencingAggregate = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.Aggregate && 
            n.FullName?.Contains("SelfReferencingAggregate") == true);
        Assert.NotNull(selfReferencingAggregate);

        // 验证对应的领域事件也被正确识别
        var recursiveEventA = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.DomainEvent && 
            n.FullName?.Contains("RecursiveEventA") == true);
        Assert.NotNull(recursiveEventA);

        var recursiveEventB = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.DomainEvent && 
            n.FullName?.Contains("RecursiveEventB") == true);
        Assert.NotNull(recursiveEventB);

        var recursiveEventC = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.DomainEvent && 
            n.FullName?.Contains("RecursiveEventC") == true);
        Assert.NotNull(recursiveEventC);

        var selfReferencingEvent = result.Nodes.FirstOrDefault(n => 
            n.Type == NodeType.DomainEvent && 
            n.FullName?.Contains("SelfReferencingEvent") == true);
        Assert.NotNull(selfReferencingEvent);

        // 验证聚合根到领域事件的关系被正确建立（没有重复）
        var aggregateToEventRelationships = result.Relationships
            .Where(r => r.Type == RelationshipType.AggregateToDomainEvent)
            .ToList();

        // 确保没有重复的关系
        var distinctRelationships = aggregateToEventRelationships
            .GroupBy(r => (r.FromNode.Id, r.ToNode.Id, r.Type))
            .Where(g => g.Count() > 1)
            .ToList();
        
        Assert.Empty(distinctRelationships); // 不应该有重复的关系
    }

    [Fact]
    public void AnalyzeFromAssemblies_WithLargeNumberOfAttributes_ShouldCompleteWithoutStackOverflow()
    {
        // 这个测试模拟大量attributes的场景（类似于用户报告的700多个attributes）
        // 通过多次包含同一个程序集来模拟大量metadata
        var assembly = Assembly.GetExecutingAssembly();
        
        // 创建多个程序集实例来模拟大量的元数据
        var assemblies = new Assembly[10]; // 模拟多个程序集
        for (int i = 0; i < assemblies.Length; i++)
        {
            assemblies[i] = assembly;
        }

        // 记录开始时间
        var startTime = DateTime.UtcNow;
        
        // 如果递归检测有问题，这里会导致栈溢出或极慢的性能
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(assemblies);
        
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        Assert.NotNull(result);
        Assert.NotNull(result.Nodes);
        Assert.NotNull(result.Relationships);
        
        // 验证在合理时间内完成（不应该超过30秒）
        Assert.True(duration.TotalSeconds < 30, $"Analysis took too long: {duration.TotalSeconds} seconds");
        
        // 验证结果不为空
        Assert.True(result.Nodes.Count > 0, "Should have found some nodes");
        Assert.True(result.Relationships.Count > 0, "Should have found some relationships");

        // 验证关系去重正常工作（不应该有大量重复的关系）
        var totalRelationships = result.Relationships.Count;
        var distinctRelationshipCount = result.Relationships
            .GroupBy(r => (r.FromNode.Id, r.ToNode.Id, r.Type))
            .Count();
        
        Assert.Equal(totalRelationships, distinctRelationshipCount); // 关系应该已经去重
    }
}