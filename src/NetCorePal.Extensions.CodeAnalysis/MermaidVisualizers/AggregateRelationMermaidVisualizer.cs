using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

/// <summary>
/// 聚合关系图可视化器，用于生成聚合间的关系图
/// </summary>
public static class AggregateRelationMermaidVisualizer
{
    // 只关注的节点类型
    static readonly HashSet<NodeType> FocusedNodeTypes = new()
    {
        NodeType.Controller,
        NodeType.Endpoint,
        NodeType.CommandSender,
        NodeType.Command,
        NodeType.Aggregate,
        NodeType.DomainEvent,
        NodeType.IntegrationEvent,
        NodeType.DomainEventHandler,
        NodeType.IntegrationEventHandler
    };

    // 只关注的关系类型（只要两端节点都在关注类型内即可）
    static bool IsFocusedRelation(Node from, Node to)
    {
        return from != null && to != null && FocusedNodeTypes.Contains(from.Type) && FocusedNodeTypes.Contains(to.Type);
    }

    /// <summary>
    /// 生成所有聚合关系图集合（每个聚合根对应一张图）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>包含所有聚合关系图的元组列表，每个聚合根对应一张图</returns>
    public static List<(string AggregateName, string Diagram)> GenerateAllAggregateMermaid(
        CodeFlowAnalysisResult analysisResult)
    {
        var result = new List<(string AggregateName, string Diagram)>();
        // 筛选所有聚合根节点
        var aggregateNodes = analysisResult.Nodes
            .Where(n => n.Type == NodeType.Aggregate)
            .ToList();
        foreach (var aggregate in aggregateNodes)
        {
            var diagram = GenerateMermaid(analysisResult, aggregate.FullName);
            result.Add((aggregate.Name, diagram));
        }
        return result;
    }

    /// <summary>
    /// 生成聚合关系图（以指定聚合为核心，遇到其它聚合则作为结束节点）新版实现
    /// </summary>
    /// <param name="analysisResult">新版代码分析结果</param>
    /// <param name="aggregateFullName">核心聚合的 FullName</param>
    /// <returns>Mermaid 图字符串</returns>
    public static string GenerateMermaid(CodeFlowAnalysisResult analysisResult,
        string aggregateFullName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");
        sb.AppendLine();

        string GetNodeId(string fullName)
        {
            return MermaidVisualizerHelper.SanitizeClassName(MermaidVisualizerHelper.GetClassNameFromFullName(fullName));
        }

        var mainPathNodes = new HashSet<string>();
        var mainPathLinks = new List<(string fromNodeId, string toNodeId, RelationshipType relType)>();

        // 找到所有上游路径经过主聚合的节点
        var upstreamFromMainAggregate = new HashSet<string>();
        // 找到所有下游路径经过主聚合的节点  
        var downstreamToMainAggregate = new HashSet<string>();
        
        // 从主聚合开始向下游查找所有可达节点
        void FindDownstreamNodes(string currentFullName, HashSet<string> visited)
        {
            if (visited.Contains(currentFullName)) return;
            visited.Add(currentFullName);
            
            upstreamFromMainAggregate.Add(currentFullName);
            
            foreach (var rel in analysisResult.Relationships.Where(r => 
                r.FromNode != null && r.FromNode.FullName == currentFullName && 
                r.ToNode != null && IsFocusedRelation(r.FromNode, r.ToNode)))
            {
                var targetNode = rel.ToNode;
                // 如果目标节点是其他聚合，包含该聚合但不继续向下
                if (targetNode.Type == NodeType.Aggregate && targetNode.FullName != aggregateFullName)
                {
                    upstreamFromMainAggregate.Add(targetNode.FullName);
                }
                else
                {
                    FindDownstreamNodes(targetNode.FullName, visited);
                }
            }
        }
        
        // 从主聚合开始向上游查找所有可达节点
        void FindUpstreamNodes(string currentFullName, HashSet<string> visited)
        {
            if (visited.Contains(currentFullName)) return;
            visited.Add(currentFullName);
            
            downstreamToMainAggregate.Add(currentFullName);
            
            foreach (var rel in analysisResult.Relationships.Where(r => 
                r.ToNode != null && r.ToNode.FullName == currentFullName && 
                r.FromNode != null && IsFocusedRelation(r.FromNode, r.ToNode)))
            {
                var sourceNode = rel.FromNode;
                // 如果源节点是其他聚合，包含该聚合但不继续向上
                if (sourceNode.Type == NodeType.Aggregate && sourceNode.FullName != aggregateFullName)
                {
                    downstreamToMainAggregate.Add(sourceNode.FullName);
                }
                else
                {
                    FindUpstreamNodes(sourceNode.FullName, visited);
                }
            }
        }

        // 从主聚合开始查找上游和下游的所有节点
        FindDownstreamNodes(aggregateFullName, new HashSet<string>());
        FindUpstreamNodes(aggregateFullName, new HashSet<string>());
        
        // 合并所有有效节点（上游经过主聚合或下游经过主聚合的节点）
        var validNodes = new HashSet<string>(upstreamFromMainAggregate);
        validNodes.UnionWith(downstreamToMainAggregate);

        // 收集所有有效的节点
        foreach (var nodeFullName in validNodes)
        {
            var node = analysisResult.Nodes.FirstOrDefault(n => n.FullName == nodeFullName && FocusedNodeTypes.Contains(n.Type));
            if (node != null)
            {
                mainPathNodes.Add(nodeFullName);
            }
        }

        // 收集所有有效的链路（两端都在有效节点集合中）
        foreach (var rel in analysisResult.Relationships.Where(r => 
            r.FromNode != null && r.ToNode != null && 
            IsFocusedRelation(r.FromNode, r.ToNode) &&
            validNodes.Contains(r.FromNode.FullName) &&
            validNodes.Contains(r.ToNode.FullName)))
        {
            var fromNodeId = GetNodeId(rel.FromNode.FullName);
            var toNodeId = GetNodeId(rel.ToNode.FullName);
            var linkTuple = (fromNodeId, toNodeId, rel.Type);
            if (!mainPathLinks.Contains(linkTuple))
                mainPathLinks.Add(linkTuple);
        }

        // 输出节点
        foreach (var currentFullName in mainPathNodes)
        {
            var node = analysisResult.Nodes.FirstOrDefault(n => n.FullName == currentFullName && FocusedNodeTypes.Contains(n.Type));
            if (node == null) continue;
            string nodeId = GetNodeId(currentFullName);
            string nodeLabel = node.Name;
            if (node.Type == NodeType.Aggregate)
            {
                sb.AppendLine($"    {nodeId}{{{{{MermaidVisualizerHelper.EscapeMermaidText(nodeLabel)}}}}}");
                // 主聚合使用特殊样式
                if (node.FullName == aggregateFullName)
                {
                    sb.AppendLine($"    class {nodeId} mainaggregate;");
                }
                else
                {
                    sb.AppendLine($"    class {nodeId} aggregate;");
                }
            }
            else
            {
                switch (node.Type)
                {
                    case NodeType.Controller:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} controller;");
                        break;
                    case NodeType.Endpoint:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} endpoint;");
                        break;
                    case NodeType.Command:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} command;");
                        break;
                    case NodeType.DomainEvent:
                        sb.AppendLine($"    {nodeId}(\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\")");
                        sb.AppendLine($"    class {nodeId} domainevent;");
                        break;
                    case NodeType.IntegrationEvent:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} integrationevent;");
                        break;
                    case NodeType.DomainEventHandler:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(MermaidVisualizerHelper.GetClassNameFromFullName(node.FullName))}\"]");
                        sb.AppendLine($"    class {nodeId} domaineventhandler;");
                        break;
                    case NodeType.IntegrationEventHandler:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(MermaidVisualizerHelper.GetClassNameFromFullName(node.FullName))}\"]");
                        sb.AppendLine($"    class {nodeId} integrationeventhandler;");
                        break;
                    case NodeType.CommandSender:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} commandsender;");
                        break;
                }
            }
        }

        // 输出链路
        foreach (var link in mainPathLinks)
        {
            var (fromNodeId, toNodeId, relType) = link;
            var label = MermaidVisualizerHelper.GetRelationshipLabel(relType);
            if (!string.IsNullOrEmpty(label))
                sb.AppendLine($"    {fromNodeId} --{MermaidVisualizerHelper.EscapeMermaidText(label)}--> {toNodeId}");
            else
                sb.AppendLine($"    {fromNodeId} --> {toNodeId}");
        }

        sb.AppendLine();
        sb.AppendLine("    %% Styles");
        sb.AppendLine("    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef endpoint fill:#e1f5fe,stroke:#01579b,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef commandsender fill:#fff8e1,stroke:#f57f17,stroke-width:2px,font-style:italic;");
        sb.AppendLine("    classDef command fill:#f3e5f5,stroke:#4a148c,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef mainaggregate fill:#ffecb3,stroke:#ff8f00,stroke-width:4px,font-weight:bold,font-size:16px;");
        sb.AppendLine("    classDef aggregate fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px;");
        sb.AppendLine("    classDef domainevent fill:#fff3e0,stroke:#e65100,stroke-width:2px,font-style:italic;");
        sb.AppendLine("    classDef integrationevent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef domaineventhandler fill:#e8eaf6,stroke:#3f51b5,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef integrationeventhandler fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef integrationeventconverter fill:#e3f2fd,stroke:#0277bd,stroke-width:2px;");

        return sb.ToString();
    }
}