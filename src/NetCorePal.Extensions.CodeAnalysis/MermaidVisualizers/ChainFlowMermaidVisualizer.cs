using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

public static class ChainFlowMermaidVisualizer
{
    /// <summary>
    /// 生成所有独立链路流程图的集合
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>包含所有独立链路图的元组列表，每个链路对应一张图</returns>
    public static List<(string ChainName, string Diagram)> GenerateAllChainFlowCharts(
        CodeFlowAnalysisResult analysisResult)
    {
        // 只以指定类型且无上游节点的节点为链路起点
        var result = new List<(string ChainName, string Diagram)>();
        var startTypes = new[]
        {
            NodeType.ControllerMethod,
            NodeType.Endpoint,
            NodeType.CommandSenderMethod,
            NodeType.Command,
            NodeType.DomainEvent,
            NodeType.IntegrationEvent,
            NodeType.DomainEventHandler,
            NodeType.IntegrationEventHandler
        };
        var allNodes = analysisResult.Nodes.ToList();
        var nodesWithUpstream = new HashSet<string>(
            analysisResult.Relationships
                .Where(r => r.ToNode != null)
                .Select(r => r.ToNode.FullName)
                .Where(n => !string.IsNullOrEmpty(n))
        );
        var startNodes = allNodes
            .Where(n => startTypes.Contains(n.Type) && !nodesWithUpstream.Contains(n.FullName))
            .ToList();
        // 只关注指定类型的关系（强类型）
        var allowedRelationTypes = new[]
        {
            RelationshipType.ControllerMethodToCommand,
            RelationshipType.EndpointToCommand,
            RelationshipType.CommandSenderMethodToCommand,
            RelationshipType.CommandToEntityMethod,
            RelationshipType.EntityMethodToEntityMethod,
            RelationshipType.EntityMethodToDomainEvent,
            RelationshipType.DomainEventToHandler,
            RelationshipType.DomainEventHandlerToCommand,
            RelationshipType.IntegrationEventToHandler,
            RelationshipType.IntegrationEventHandlerToCommand,
            RelationshipType.DomainEventToIntegrationEvent
        };
        foreach (var start in startNodes)
        {
            var chainNodes = new HashSet<string>();
            var chainRelations = new List<(string source, string target, string label)>();
            var chainNodeIds = new Dictionary<string, string>();
            int nodeIdCounter = 1;

            void Traverse(string fullName)
            {
                if (string.IsNullOrEmpty(fullName) || chainNodes.Contains(fullName)) return;
                chainNodes.Add(fullName);
                var node = analysisResult.Nodes.FirstOrDefault(n => n.FullName == fullName);
                if (node != null && !chainNodeIds.ContainsKey(fullName))
                    chainNodeIds[fullName] = $"N{nodeIdCounter++}";

                foreach (var rel in analysisResult.Relationships)
                {
                    if (rel.FromNode == null || rel.ToNode == null) continue;
                    if (rel.FromNode.FullName == fullName && allowedRelationTypes.Contains(rel.Type))
                    {
                        chainRelations.Add((rel.FromNode.FullName, rel.ToNode.FullName,
                            MermaidVisualizerHelper.GetRelationshipLabel(rel.Type)));
                        Traverse(rel.ToNode.FullName);
                    }
                }
            }

            Traverse(start.FullName);

            var sb = new StringBuilder();
            sb.AppendLine("flowchart TD");
            sb.AppendLine();
            sb.AppendLine($"    %% {MermaidVisualizerHelper.EscapeMermaidText(start.Name)} Chain");
            sb.AppendLine();

            // 添加所有节点
            foreach (var nodeFullName in chainNodes)
            {
                var node = analysisResult.Nodes.FirstOrDefault(n => n.FullName == nodeFullName);
                if (node != null && chainNodeIds.TryGetValue(nodeFullName, out var nodeId))
                {
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} {node.Type.ToString().ToLower()};");
                }
            }

            sb.AppendLine();
            sb.AppendLine("    %% Chain Relationships");

            foreach (var (source, target, label) in chainRelations)
            {
                var sourceNodeId = chainNodeIds.TryGetValue(source, out var srcId) ? srcId : string.Empty;
                var targetNodeId = chainNodeIds.TryGetValue(target, out var tgtId) ? tgtId : string.Empty;
                if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
                {
                    sb.AppendLine($"    {sourceNodeId} -->|{label}| {targetNodeId}");
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

            result.Add((start.Name, sb.ToString()));
        }

        return result;
    }
}