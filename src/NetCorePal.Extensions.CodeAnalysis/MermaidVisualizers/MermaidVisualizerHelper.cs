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

    public static string GetRelationshipLabel(RelationshipType relationshipType)
    {
        return relationshipType switch
        {
            RelationshipType.ControllerToCommand => "Send",
            RelationshipType.ControllerMethodToCommand => "Send",
            RelationshipType.EndpointToCommand => "Send",
            RelationshipType.CommandSenderToCommand => "Send",
            RelationshipType.CommandSenderMethodToCommand => "Send",
            RelationshipType.CommandToAggregate => "Invoke",
            RelationshipType.CommandToEntityMethod => "Invoke",
            RelationshipType.AggregateToDomainEvent => "Publish",
            RelationshipType.EntityMethodToEntityMethod => "Invoke",
            RelationshipType.EntityMethodToDomainEvent => "Publish",
            RelationshipType.DomainEventToHandler => "Handle",
            RelationshipType.DomainEventHandlerToCommand => "Send",
            RelationshipType.IntegrationEventToHandler => "Handle",
            RelationshipType.IntegrationEventHandlerToCommand => "Send",
            RelationshipType.DomainEventToIntegrationEvent => "Convert",
            _ => throw new ArgumentOutOfRangeException(nameof(relationshipType), relationshipType, null)
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
    #endregion
}