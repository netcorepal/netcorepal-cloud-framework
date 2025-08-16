using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

/// <summary>
/// 架构总览图（展示类型间的关系）Mermaid 可视化器
/// </summary>
public static class ArchitectureOverviewMermaidVisualizer
{
    /// <summary>
    /// 生成架构总览图（展示类型间的关系）Mermaid 可视化器
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>Mermaid 类图字符串</returns>
    public static string GenerateMermaid(CodeFlowAnalysisResult analysisResult)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");
        sb.AppendLine();

        var nodeTypes = new Dictionary<string, string>();
        foreach (var node in analysisResult.Nodes)
        {
            var nodeName = MermaidVisualizerHelper.SanitizeClassName(node.Name);
            var displayName = MermaidVisualizerHelper.EscapeMermaidText(node.Name);
            switch (node.Type)
            {
                case NodeType.Controller:
                    sb.AppendLine($"    {nodeName}[\"{displayName}\"]");
                    sb.AppendLine($"    class {nodeName} controller;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.Endpoint:
                    sb.AppendLine($"    {nodeName}[\"{displayName}\"]");
                    sb.AppendLine($"    class {nodeName} endpoint;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.CommandSender:
                    sb.AppendLine($"    {nodeName}[\"{displayName}\"]");
                    sb.AppendLine($"    class {nodeName} commandSender;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.Command:
                    sb.AppendLine($"    {nodeName}[\"{displayName}\"]");
                    sb.AppendLine($"    class {nodeName} command;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.Aggregate:
                    sb.AppendLine($"    {nodeName}{{{{{displayName}}}}}");
                    sb.AppendLine($"    class {nodeName} aggregate;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.DomainEvent:
                    sb.AppendLine($"    {nodeName}(\"{displayName}\")");
                    sb.AppendLine($"    class {nodeName} domainEvent;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.IntegrationEvent:
                    sb.AppendLine($"    {nodeName}(\"{displayName}\")");
                    sb.AppendLine($"    class {nodeName} integrationEvent;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.DomainEventHandler:
                    sb.AppendLine($"    {nodeName}[\"{displayName}\"]");
                    sb.AppendLine($"    class {nodeName} domaineventhandler;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.IntegrationEventHandler:
                    sb.AppendLine($"    {nodeName}[\"{displayName}\"]");
                    sb.AppendLine($"    class {nodeName} integrationeventhandler;");
                    nodeTypes[node.Name] = nodeName;
                    break;
                case NodeType.IntegrationEventConverter:
                    sb.AppendLine($"    {nodeName}[\"{displayName}\"]");
                    sb.AppendLine($"    class {nodeName} converter;");
                    nodeTypes[node.Name] = nodeName;
                    break;
            }
        }

        // 只输出签名分析相关节点类型之间的关系
        var allowedTypes = new[]
        {
            NodeType.Controller,
            NodeType.Endpoint,
            NodeType.CommandSender,
            NodeType.Command,
            NodeType.Aggregate,
            NodeType.DomainEvent,
            NodeType.IntegrationEvent,
            NodeType.DomainEventHandler,
            NodeType.IntegrationEventHandler,
            NodeType.IntegrationEventConverter
        };
        var processedLinks = new HashSet<string>();
        foreach (var rel in analysisResult.Relationships)
        {
            if (rel.FromNode == null || rel.ToNode == null) continue;
            if (!allowedTypes.Contains(rel.FromNode.Type) || !allowedTypes.Contains(rel.ToNode.Type)) continue;
            
            // 使用清理后的节点名称
            if (!nodeTypes.TryGetValue(rel.FromNode.Name, out var sourceName) ||
                !nodeTypes.TryGetValue(rel.ToNode.Name, out var targetName)) continue;
                
            var linkKey = $"{sourceName}->{targetName}:{MermaidVisualizerHelper.GetRelationshipLabel(rel.Type)}";
            if (!string.IsNullOrEmpty(sourceName) && !string.IsNullOrEmpty(targetName) &&
                !processedLinks.Contains(linkKey))
            {
                processedLinks.Add(linkKey);
                sb.AppendLine(
                    $"    {sourceName} -->|{MermaidVisualizerHelper.GetRelationshipLabel(rel.Type)}| {targetName}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("    %% Styles");
        sb.AppendLine("    classDef controller fill:#e3f2fd,stroke:#1565c0,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef endpoint fill:#e3f2fd,stroke:#1976d2,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef commandSender fill:#e3f2fd,stroke:#42a5f5,stroke-width:2px,font-style:italic;");
        sb.AppendLine("    classDef command fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef aggregate fill:#fff8e1,stroke:#ff8f00,stroke-width:3px,font-weight:bold;");
        sb.AppendLine("    classDef domainEvent fill:#fff3e0,stroke:#e65100,stroke-width:2px,font-style:italic;");
        sb.AppendLine("    classDef domaineventhandler fill:#fff3e0,stroke:#ef6c00,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef integrationEvent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef integrationeventhandler fill:#fce4ec,stroke:#ad1457,stroke-width:2px,font-weight:bold;");
        sb.AppendLine("    classDef converter fill:#e3f2fd,stroke:#0277bd,stroke-width:2px;");

        return sb.ToString();
    }
}