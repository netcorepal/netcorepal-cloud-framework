using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

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
    /// 生成所有聚合关系图的集合（新版，基于 CodeFlowAnalysisResult）
    /// </summary>
    /// <param name="analysisResult">新版代码分析结果</param>
    /// <returns>包含所有聚合关系图的元组列表，每个聚合根对应一张图</returns>
    public static List<(string AggregateName, string Diagram)> GenerateAllAggregateRelationDiagrams(
        CodeFlowAnalysisResult analysisResult)
    {
        var result = new List<(string AggregateName, string Diagram)>();
        // 筛选所有聚合根节点
        var aggregateNodes = analysisResult.Nodes
            .Where(n => n.Type == NodeType.Aggregate)
            .ToList();
        foreach (var aggregate in aggregateNodes)
        {
            var diagram = GenerateAggregateRelationDiagram(analysisResult, aggregate.FullName);
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
    public static string GenerateAggregateRelationDiagram(CodeFlowAnalysisResult analysisResult,
        string aggregateFullName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");
        sb.AppendLine();

        string GetNodeId(string fullName)
        {
            return MermaidVisualizerHelper.SanitizeClassName(MermaidVisualizerHelper.GetClassNameFromFullName(fullName));
        }

        var processedNodes = new HashSet<string>();
        var processedLinks = new HashSet<string>();
        var mainPathNodes = new HashSet<string>();
        var mainPathLinks = new HashSet<string>();

        // 只收集所有"链路上包含主聚合"的节点和连线，且只处理关注类型
        void Traverse(string currentFullName, HashSet<string> path, bool onMainAggregatePath)
        {
            if (path.Contains(currentFullName)) return;
            path.Add(currentFullName);

            var node = analysisResult.Nodes.FirstOrDefault(n => n.FullName == currentFullName && FocusedNodeTypes.Contains(n.Type));
            bool isMainAggregate = (currentFullName == aggregateFullName);
            bool isOtherAggregate = node != null && node.Type == NodeType.Aggregate && node.FullName != aggregateFullName;

            // 如果当前在主聚合路径上，或者当前就是主聚合，则收集节点
            bool shouldCollect = onMainAggregatePath || isMainAggregate;

            string nodeId = GetNodeId(currentFullName);
            string nodeLabel = node != null ? node.Name : MermaidVisualizerHelper.GetClassNameFromFullName(currentFullName);

            // 收集节点
            if (shouldCollect && !mainPathNodes.Contains(currentFullName) && node != null && FocusedNodeTypes.Contains(node.Type))
            {
                mainPathNodes.Add(currentFullName);
            }

            // 递归下游
            foreach (var rel in analysisResult.Relationships.Where(r => r.FromNode != null && r.FromNode.FullName == currentFullName && r.ToNode != null && IsFocusedRelation(r.FromNode, r.ToNode)))
            {
                var targetNode = rel.ToNode;
                bool targetIsOtherAggregate = targetNode != null && targetNode.Type == NodeType.Aggregate && targetNode.FullName != aggregateFullName;
                string targetNodeId = GetNodeId(targetNode?.FullName ?? "");

                // 收集连线
                if (shouldCollect)
                {
                    var linkKey = $"{nodeId}->{targetNodeId}";
                    mainPathLinks.Add(linkKey);
                }

                // 如果目标是非主聚合，不递归其下游
                if (targetIsOtherAggregate) continue;

                // 递归下游：如果当前在主聚合路径上，或者当前是主聚合，则下游也在主聚合路径上
                bool nextOnMainPath = onMainAggregatePath || isMainAggregate;
                Traverse(targetNode?.FullName ?? "", path, nextOnMainPath);
            }

            // 递归上游
            foreach (var rel in analysisResult.Relationships.Where(r => r.ToNode != null && r.ToNode.FullName == currentFullName && r.FromNode != null && IsFocusedRelation(r.FromNode, r.ToNode)))
            {
                var sourceNode = rel.FromNode;
                bool sourceIsOtherAggregate = sourceNode != null && sourceNode.Type == NodeType.Aggregate && sourceNode.FullName != aggregateFullName;
                string sourceNodeId = GetNodeId(sourceNode?.FullName ?? "");

                // 如果当前节点是"非主聚合"，则不展示其上游
                if (isOtherAggregate)
                    continue;

                // 收集连线
                if (shouldCollect)
                {
                    var linkKey = $"{sourceNodeId}->{nodeId}";
                    mainPathLinks.Add(linkKey);
                }

                // 如果源是非主聚合，不递归其上游
                if (sourceIsOtherAggregate) continue;

                // 递归上游：如果当前在主聚合路径上，或者当前是主聚合，则上游也在主聚合路径上
                bool nextOnMainPath = onMainAggregatePath || isMainAggregate;
                Traverse(sourceNode?.FullName ?? "", path, nextOnMainPath);
            }
        }

        // 从主聚合开始，递归所有相关链路
        Traverse(aggregateFullName, new HashSet<string>(), true);

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
                sb.AppendLine($"    class {nodeId} aggregate;");
            }
            else
            {
                switch (node.Type)
                {
                    case NodeType.Controller:
                    case NodeType.Endpoint:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} controller;");
                        break;
                    case NodeType.Command:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} command;");
                        break;
                    case NodeType.DomainEvent:
                        sb.AppendLine($"    {nodeId}(\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\")");
                        sb.AppendLine($"    class {nodeId} domainEvent;");
                        break;
                    case NodeType.IntegrationEvent:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} integrationEvent;");
                        break;
                    case NodeType.DomainEventHandler:
                    case NodeType.IntegrationEventHandler:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(MermaidVisualizerHelper.GetClassNameFromFullName(node.FullName))}\"]");
                        sb.AppendLine($"    class {nodeId} handler;");
                        break;
                    case NodeType.CommandSender:
                        sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} commandSender;");
                        break;
                }
            }
        }

        // 输出链路
        foreach (var link in mainPathLinks)
        {
            var arr = link.Split("->");
            if (arr.Length == 2)
            {
                sb.AppendLine($"    {arr[0]} --> {arr[1]}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("    %% Styles");
        sb.AppendLine("    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef commandSender fill:#fff8e1,stroke:#f57f17,stroke-width:2px,font-style:italic;");
        sb.AppendLine("    classDef command fill:#f3e5f5,stroke:#4a148c,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef entity fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px;");
        sb.AppendLine("    classDef domainEvent fill:#fff3e0,stroke:#e65100,stroke-width:2px,font-style:italic;");
        sb.AppendLine("    classDef integrationEvent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef handler fill:#f1f8e9,stroke:#33691e,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef converter fill:#e3f2fd,stroke:#0277bd,stroke-width:2px;");

        return sb.ToString();
    }
}