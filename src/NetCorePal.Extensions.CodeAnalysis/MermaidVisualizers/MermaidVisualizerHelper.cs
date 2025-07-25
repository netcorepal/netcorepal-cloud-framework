using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.MermaidVisualizers;

/// <summary>
/// Mermaid 图表可视化器，用于将代码分析结果转换为 Mermaid 图表
/// </summary>
public static class MermaidVisualizerHelper
{
    #region 辅助方法

    public static string GetRelationshipLabel(string callType, string sourceMethod = "", string targetMethod = "")
    {
        return callType switch
        {
            "MethodToCommand" => string.IsNullOrEmpty(sourceMethod) ? "sends" : $"{sourceMethod} Send",
            "CommandToAggregateMethod" => string.IsNullOrEmpty(targetMethod) ? "executes" : $"executes {targetMethod}",
            "DomainEventToHandler" => "handles",
            "DomainEventToIntegrationEvent" => "converts to",
            "IntegrationEventToHandler" => "subscribes",
            "HandlerToCommand" => "sends",
            _ => ""
        };
    }


    public static string EscapeMermaidText(string text)
    {
        return text.Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\n", " ");
    }

    public static string SanitizeClassName(string className)
    {
        return className.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("[", "_").Replace("]", "_");
    }

    public static string GetClassNameFromFullName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        var parts = fullName.Split('.');
        return parts.LastOrDefault() ?? "";
    }
    
    /// <summary>
    /// 解析节点名称，分离类型和方法
    /// </summary>
    private static (string NodeType, string Method) ParseNodeName(string nodeFullName)
    {
        if (nodeFullName.Contains("::"))
        {
            var parts = nodeFullName.Split(new[] { "::" }, StringSplitOptions.None);
            return (parts[0], parts[1]);
        }

        return (nodeFullName, "");
    }

    /// <summary>
    /// 获取节点样式类
    /// </summary>
    public static string GetNodeStyleClass(string nodeFullName, CodeFlowAnalysisResult analysisResult)
    {
        var className = GetClassNameFromFullName(nodeFullName);

        if (analysisResult.Controllers.Any(c => c.FullName == nodeFullName))
            return "controller";
        if (analysisResult.CommandSenders.Any(cs => cs.FullName == nodeFullName))
            return "commandSender";
        if (analysisResult.Commands.Any(c => c.FullName == nodeFullName))
            return "command";
        if (analysisResult.Entities.Any(e => e.FullName == nodeFullName))
            return "entity";
        if (analysisResult.DomainEvents.Any(d => d.FullName == nodeFullName))
            return "domainEvent";
        if (analysisResult.IntegrationEvents.Any(i => i.FullName == nodeFullName))
            return "integrationEvent";
        if (analysisResult.DomainEventHandlers.Any(h => h.FullName == nodeFullName))
            return "handler";
        if (analysisResult.IntegrationEventHandlers.Any(h => h.FullName == nodeFullName))
            return "handler";
        if (analysisResult.IntegrationEventConverters.Any(c => c.FullName == nodeFullName))
            return "converter";

        return "default";
    }
    #endregion
}