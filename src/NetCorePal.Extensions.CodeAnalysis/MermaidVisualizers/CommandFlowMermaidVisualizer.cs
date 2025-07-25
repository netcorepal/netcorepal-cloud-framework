
using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

public static class CommandFlowMermaidVisualizer
{
    /// <summary>
    /// 生成命令流程图（专注于命令执行流程）新版实现
    /// </summary>
    /// <param name="analysisResult2">新版代码分析结果</param>
    /// <returns>Mermaid 流程图字符串</returns>
    public static string GenerateCommandFlowChart(CodeFlowAnalysisResult2 analysisResult2)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");
        sb.AppendLine();

        var nodeIds = new Dictionary<string, string>();
        var nodeIdCounter = 1;

        string GetNodeId(string fullName, string nodeType)
        {
            var key = $"{nodeType}_{fullName}";
            if (!nodeIds.ContainsKey(key))
            {
                nodeIds[key] = $"{nodeType}{nodeIdCounter++}";
            }
            return nodeIds[key];
        }

        // 只显示命令相关的流程
        var commandRelationships = analysisResult2.Relationships
            .Where(r => r.Type == RelationshipType.CommandToAggregateMethod || r.Type == RelationshipType.CommandSenderMethodToCommand || r.Type == RelationshipType.ControllerToCommand || r.Type == RelationshipType.EndpointToCommand)
            .ToList();

        var involvedNodeFullNames = new HashSet<string>();
        foreach (var rel in commandRelationships)
        {
            if (rel.FromNode != null) involvedNodeFullNames.Add(rel.FromNode.FullName);
            if (rel.ToNode != null) involvedNodeFullNames.Add(rel.ToNode.FullName);
        }

        // 添加相关的控制器
        foreach (var node in analysisResult2.Nodes.Where(n => involvedNodeFullNames.Contains(n.FullName) && (n.Type == NodeType.ControllerMethod || n.Type == NodeType.Endpoint)))
        {
            var nodeId = GetNodeId(node.FullName, "C");
            sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
            sb.AppendLine($"    class {nodeId} controller;");
        }

        // 添加相关的命令发送者（除了已经作为控制器显示的）
        var controllerFullNames = new HashSet<string>(analysisResult2.Nodes.Where(n => n.Type == NodeType.ControllerMethod || n.Type == NodeType.Endpoint).Select(n => n.FullName));
        foreach (var node in analysisResult2.Nodes.Where(n => involvedNodeFullNames.Contains(n.FullName) && n.Type == NodeType.CommandSender && !controllerFullNames.Contains(n.FullName)))
        {
            var nodeId = GetNodeId(node.FullName, "CS");
            sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
            sb.AppendLine($"    class {nodeId} commandSender;");
        }

        // 添加相关的命令
        foreach (var node in analysisResult2.Nodes.Where(n => involvedNodeFullNames.Contains(n.FullName) && n.Type == NodeType.Command))
        {
            var nodeId = GetNodeId(node.FullName, "CMD");
            sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
            sb.AppendLine($"    class {nodeId} command;");
        }

        // 添加相关的聚合
        foreach (var node in analysisResult2.Nodes.Where(n => involvedNodeFullNames.Contains(n.FullName) && n.Type == NodeType.Aggregate))
        {
            var nodeId = GetNodeId(node.FullName, "AGG");
            sb.AppendLine($"    {nodeId}{{{{{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}}}}}");
            sb.AppendLine($"    class {nodeId} aggregate;");
        }

        sb.AppendLine();

        // 添加命令流程关系，去重每对节点之间的连线
        var processedLinks = new HashSet<string>();
        foreach (var relationship in commandRelationships)
        {
            var sourceNodeId = relationship.FromNode != null ? GetNodeId(relationship.FromNode.FullName, GetNodeTypeShort(relationship.FromNode.Type)) : "";
            var targetNodeId = relationship.ToNode != null ? GetNodeId(relationship.ToNode.FullName, GetNodeTypeShort(relationship.ToNode.Type)) : "";

            if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
            {
                var linkKey = $"{sourceNodeId}->{targetNodeId}";
                if (!processedLinks.Contains(linkKey))
                {
                    processedLinks.Add(linkKey);
                    var label = "call"; // 简化的标签
                    sb.AppendLine($"    {sourceNodeId} --> |{label}| {targetNodeId}");
                }
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

    private static string GetNodeTypeShort(NodeType type)
    {
        return type switch
        {
            NodeType.ControllerMethod => "C",
            NodeType.Endpoint => "C",
            NodeType.CommandSender => "CS",
            NodeType.Command => "CMD",
            NodeType.Aggregate => "AGG",
            _ => "N"
        };
    }
}
