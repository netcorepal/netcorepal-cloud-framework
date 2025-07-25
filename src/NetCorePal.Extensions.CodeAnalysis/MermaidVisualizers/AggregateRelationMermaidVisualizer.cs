using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

public static class AggregateRelationMermaidVisualizer
{
    
    /// <summary>
    /// 生成所有聚合关系图的集合（新版，基于 CodeFlowAnalysisResult2）
    /// </summary>
    /// <param name="analysisResult2">新版代码分析结果</param>
    /// <returns>包含所有聚合关系图的元组列表，每个聚合根对应一张图</returns>
    public static List<(string AggregateName, string Diagram)> GenerateAllAggregateRelationDiagrams(
        CodeFlowAnalysisResult2 analysisResult2)
    {
        var result = new List<(string AggregateName, string Diagram)>();
        // 筛选所有聚合根节点
        var aggregateNodes = analysisResult2.Nodes
            .Where(n => n.Type == NodeType.Aggregate)
            .ToList();
        foreach (var aggregate in aggregateNodes)
        {
            var diagram = GenerateAggregateRelationDiagram(analysisResult2, aggregate.FullName);
            result.Add((aggregate.Name, diagram));
        }
        return result;
    }
    
    /// <summary>
    /// 生成聚合关系图（以指定聚合为核心，遇到其它聚合则作为结束节点）新版实现
    /// </summary>
    /// <param name="analysisResult2">新版代码分析结果</param>
    /// <param name="aggregateFullName">核心聚合的 FullName</param>
    /// <returns>Mermaid 图字符串</returns>
    public static string GenerateAggregateRelationDiagram(CodeFlowAnalysisResult2 analysisResult2,
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

        // 只收集所有"链路上包含主聚合"的节点和连线
        void Traverse(string currentFullName, HashSet<string> path, bool onMainAggregatePath)
        {
            if (path.Contains(currentFullName)) return;
            path.Add(currentFullName);

            var node = analysisResult2.Nodes.FirstOrDefault(n => n.FullName == currentFullName);
            bool isMainAggregate = (currentFullName == aggregateFullName);
            bool isOtherAggregate = node != null && node.Type == NodeType.Aggregate && node.FullName != aggregateFullName;

            // 如果当前在主聚合路径上，或者当前就是主聚合，则收集节点
            bool shouldCollect = onMainAggregatePath || isMainAggregate;

            string nodeId = GetNodeId(currentFullName);
            string nodeLabel = node != null ? node.Name : MermaidVisualizerHelper.GetClassNameFromFullName(currentFullName);

            // 收集节点
            if (shouldCollect && !processedNodes.Contains(currentFullName))
            {
                if (node != null && node.Type == NodeType.Aggregate)
                {
                    sb.AppendLine($"    {nodeId}{{{{{MermaidVisualizerHelper.EscapeMermaidText(nodeLabel)}}}}}");
                    sb.AppendLine($"    class {nodeId} aggregate;");
                }
                else if (node != null)
                {
                    switch (node.Type)
                    {
                        case NodeType.ControllerMethod:
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
                        case NodeType.IntegrationEventConverter:
                            sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                            sb.AppendLine($"    class {nodeId} converter;");
                            break;
                        case NodeType.AggregateMethod:
                            sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                            sb.AppendLine($"    class {nodeId} entity;");
                            break;
                    }
                }

                processedNodes.Add(currentFullName);
            }

            // 递归下游
            foreach (var rel in analysisResult2.Relationships.Where(r => r.FromNode != null && r.FromNode.FullName == currentFullName && r.ToNode != null))
            {
                var targetNode = rel.ToNode;
                bool targetIsOtherAggregate = targetNode != null && targetNode.Type == NodeType.Aggregate && targetNode.FullName != aggregateFullName;
                string targetNodeId = GetNodeId(targetNode?.FullName ?? "");

                // 收集连线
                if (shouldCollect)
                {
                    var linkKey = $"{nodeId}->{targetNodeId}";
                    if (!processedLinks.Contains(linkKey))
                    {
                        processedLinks.Add(linkKey);
                        sb.AppendLine($"    {nodeId} --> {targetNodeId}");
                    }
                }

                // 如果目标是非主聚合，不递归其下游
                if (targetIsOtherAggregate) continue;

                // 递归下游：如果当前在主聚合路径上，或者当前是主聚合，则下游也在主聚合路径上
                bool nextOnMainPath = onMainAggregatePath || isMainAggregate;
                Traverse(targetNode?.FullName ?? "", path, nextOnMainPath);
            }

            // 递归上游
            foreach (var rel in analysisResult2.Relationships.Where(r => r.ToNode != null && r.ToNode.FullName == currentFullName && r.FromNode != null))
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
                    if (!processedLinks.Contains(linkKey))
                    {
                        processedLinks.Add(linkKey);
                        sb.AppendLine($"    {sourceNodeId} --> {nodeId}");
                    }
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