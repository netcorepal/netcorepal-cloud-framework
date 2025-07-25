using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

public static class ClassDiagramMermaidVisualizer
{
    /// <summary>
    /// 生成类图（展示类型间的关系）新版实现
    /// </summary>
    /// <param name="analysisResult2">新版代码分析结果</param>
    /// <returns>Mermaid 类图字符串</returns>
    public static string GenerateClassDiagram(CodeFlowAnalysisResult2 analysisResult2)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");
        sb.AppendLine();

        var nodeTypes = new Dictionary<string, string>();
        foreach (var node in analysisResult2.Nodes)
        {
            var nodeId = MermaidVisualizerHelper.SanitizeClassName(node.Name);
            switch (node.Type)
            {
                case NodeType.Controller:
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} controller;");
                    nodeTypes[nodeId] = "controller";
                    break;
                case NodeType.Endpoint:
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} controller;");
                    nodeTypes[nodeId] = "controller";
                    break;
                case NodeType.CommandSender:
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} commandSender;");
                    nodeTypes[nodeId] = "commandSender";
                    break;
                case NodeType.Command:
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} command;");
                    nodeTypes[nodeId] = "command";
                    break;
                case NodeType.Entity:
                    var shape = node.Properties != null && node.Properties.TryGetValue("IsAggregateRoot", out var v) && v is bool b && b
                        ? "{{" + MermaidVisualizerHelper.EscapeMermaidText(node.Name) + "}}"
                        : "[" + MermaidVisualizerHelper.EscapeMermaidText(node.Name) + "]";
                    sb.AppendLine($"    {nodeId}{shape}");
                    sb.AppendLine($"    class {nodeId} entity;");
                    nodeTypes[nodeId] = "entity";
                    break;
                case NodeType.DomainEvent:
                    sb.AppendLine($"    {nodeId}(\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\")");
                    sb.AppendLine($"    class {nodeId} domainEvent;");
                    nodeTypes[nodeId] = "domainEvent";
                    break;
                case NodeType.IntegrationEvent:
                    sb.AppendLine($"    {nodeId}(\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\")");
                    sb.AppendLine($"    class {nodeId} integrationEvent;");
                    nodeTypes[nodeId] = "integrationEvent";
                    break;
                case NodeType.DomainEventHandler:
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} handler;");
                    nodeTypes[nodeId] = "handler";
                    break;
                case NodeType.IntegrationEventHandler:
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} handler;");
                    nodeTypes[nodeId] = "handler";
                    break;
                case NodeType.IntegrationEventConverter:
                    sb.AppendLine($"    {nodeId}[\"{MermaidVisualizerHelper.EscapeMermaidText(node.Name)}\"]");
                    sb.AppendLine($"    class {nodeId} converter;");
                    nodeTypes[nodeId] = "converter";
                    break;
            }
        }

        // 连线（去重）
        var processedLinks = new HashSet<string>();
        foreach (var rel in analysisResult2.Relationships)
        {
            var sourceId = rel.FromNode != null ? MermaidVisualizerHelper.SanitizeClassName(rel.FromNode.Name) : "";
            var targetId = rel.ToNode != null ? MermaidVisualizerHelper.SanitizeClassName(rel.ToNode.Name) : "";
            var linkKey = $"{sourceId}->{targetId}";
            if (!string.IsNullOrEmpty(sourceId) && !string.IsNullOrEmpty(targetId) && !processedLinks.Contains(linkKey))
            {
                processedLinks.Add(linkKey);
                sb.AppendLine($"    {sourceId} -->|{rel.Type}| {targetId}");
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