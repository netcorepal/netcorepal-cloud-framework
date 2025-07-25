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
            var nodeName = node.Name;
            switch (node.Type)
            {
                case NodeType.Controller:
                    sb.AppendLine($"    {nodeName}[\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\"]");
                    sb.AppendLine($"    class {nodeName} controller;");
                    nodeTypes[nodeName] = "controller";
                    break;
                case NodeType.Endpoint:
                    sb.AppendLine($"    {nodeName}[\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\"]");
                    sb.AppendLine($"    class {nodeName} controller;");
                    nodeTypes[nodeName] = "controller";
                    break;
                case NodeType.CommandSender:
                    sb.AppendLine($"    {nodeName}[\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\"]");
                    sb.AppendLine($"    class {nodeName} commandSender;");
                    nodeTypes[nodeName] = "commandSender";
                    break;
                case NodeType.Command:
                    sb.AppendLine($"    {nodeName}[\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\"]");
                    sb.AppendLine($"    class {nodeName} command;");
                    nodeTypes[nodeName] = "command";
                    break;
                case NodeType.Entity:
                    var shape = node.Properties != null && node.Properties.TryGetValue("IsAggregateRoot", out var v) && v is bool b && b
                        ? "{{" + MermaidVisualizerHelper.EscapeMermaidText(nodeName) + "}}"
                        : "[" + MermaidVisualizerHelper.EscapeMermaidText(nodeName) + "]";
                    sb.AppendLine($"    {nodeName}{shape}");
                    sb.AppendLine($"    class {nodeName} entity;");
                    nodeTypes[nodeName] = "entity";
                    break;
                case NodeType.DomainEvent:
                    sb.AppendLine($"    {nodeName}(\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\")");
                    sb.AppendLine($"    class {nodeName} domainEvent;");
                    nodeTypes[nodeName] = "domainEvent";
                    break;
                case NodeType.IntegrationEvent:
                    sb.AppendLine($"    {nodeName}(\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\")");
                    sb.AppendLine($"    class {nodeName} integrationEvent;");
                    nodeTypes[nodeName] = "integrationEvent";
                    break;
                case NodeType.DomainEventHandler:
                    sb.AppendLine($"    {nodeName}[\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\"]");
                    sb.AppendLine($"    class {nodeName} handler;");
                    nodeTypes[nodeName] = "handler";
                    break;
                case NodeType.IntegrationEventHandler:
                    sb.AppendLine($"    {nodeName}[\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\"]");
                    sb.AppendLine($"    class {nodeName} handler;");
                    nodeTypes[nodeName] = "handler";
                    break;
                case NodeType.IntegrationEventConverter:
                    sb.AppendLine($"    {nodeName}[\"{MermaidVisualizerHelper.EscapeMermaidText(nodeName)}\"]");
                    sb.AppendLine($"    class {nodeName} converter;");
                    nodeTypes[nodeName] = "converter";
                    break;
            }
        }

        // 连线（去重）
        var processedLinks = new HashSet<string>();
        foreach (var rel in analysisResult2.Relationships)
        {
            var sourceName = rel.FromNode != null ? rel.FromNode.Name : "";
            var targetName = rel.ToNode != null ? rel.ToNode.Name : "";
            var linkKey = $"{sourceName}->{targetName}";
            if (!string.IsNullOrEmpty(sourceName) && !string.IsNullOrEmpty(targetName) && !processedLinks.Contains(linkKey))
            {
                processedLinks.Add(linkKey);
                sb.AppendLine($"    {sourceName} -->|{rel.Type}| {targetName}");
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