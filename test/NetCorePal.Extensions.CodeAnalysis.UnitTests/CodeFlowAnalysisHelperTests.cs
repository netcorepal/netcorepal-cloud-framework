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
        var result = CodeFlowAnalysisHelper2.GetResultFromAssemblies(assembly);

        Assert.NotNull(result);
        Assert.NotNull(result.Nodes);
        Assert.NotNull(result.Relationships);

        // 节点和关系数量应大于0
        Assert.True(result.Nodes.Count > 0, "分析结果应包含至少一个节点");
        Assert.True(result.Relationships.Count > 0, "分析结果应包含至少一个关系");

        // 断言：每种 NodeType 至少有一个节点
        foreach (NodeType nodeType in Enum.GetValues(typeof(NodeType)))
        {
            Assert.Contains(result.Nodes, n => n.Type == nodeType);
        }

        // 断言：每种 RelationshipType 至少有一个关系
        foreach (RelationshipType relType in Enum.GetValues(typeof(RelationshipType)))
        {
            Assert.Contains(result.Relationships, r => r.Type == relType);
        }

        // 可选：输出部分节点和关系信息，便于调试
        foreach (var node in result.Nodes)
        {
            // 只输出部分信息
            System.Console.WriteLine($"Node: {node.Id}, {node.Name}, {node.Type}");
        }
        foreach (var rel in result.Relationships)
        {
            System.Console.WriteLine($"Relationship: {rel.Type}, {rel.FromNode.Id} -> {rel.ToNode.Id}");
        }
    }
}
