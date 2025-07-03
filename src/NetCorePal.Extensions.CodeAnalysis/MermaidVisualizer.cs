using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// Mermaid 图表可视化器，用于将代码分析结果转换为 Mermaid 图表
/// </summary>
public static class MermaidVisualizer
{
    /// <summary>
    /// 生成完整的架构流程图
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>Mermaid 流程图字符串</returns>
    public static string GenerateArchitectureFlowChart(CodeFlowAnalysisResult analysisResult)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");
        sb.AppendLine();

        var nodeIds = new Dictionary<string, string>();
        var nodeIdCounter = 1;

        // 生成节点ID映射
        string GetNodeId(string fullName, string nodeType)
        {
            var key = $"{nodeType}_{fullName}";
            if (!nodeIds.ContainsKey(key))
            {
                nodeIds[key] = $"{nodeType}{nodeIdCounter++}";
            }
            return nodeIds[key];
        }

        // 添加控制器节点
        sb.AppendLine("    %% Controllers");
        foreach (var controller in analysisResult.Controllers)
        {
            var nodeId = GetNodeId(controller.FullName, "C");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(controller.Name)}\"]");
            sb.AppendLine($"    {nodeId} --> {nodeId}_methods[\"Methods: {string.Join(", ", controller.Methods.Take(3))}{(controller.Methods.Count > 3 ? "..." : "")}\"]");
        }
        sb.AppendLine();

        // 添加命令节点
        sb.AppendLine("    %% Commands");
        foreach (var command in analysisResult.Commands)
        {
            var nodeId = GetNodeId(command.FullName, "CMD");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(command.Name)}\"]");
        }
        sb.AppendLine();

        // 添加实体节点
        sb.AppendLine("    %% Entities");
        foreach (var entity in analysisResult.Entities)
        {
            var nodeId = GetNodeId(entity.FullName, "E");
            var shape = entity.IsAggregateRoot ? "{{" + EscapeMermaidText(entity.Name) + "}}" : "[" + EscapeMermaidText(entity.Name) + "]";
            sb.AppendLine($"    {nodeId}{shape}");
        }
        sb.AppendLine();

        // 添加领域事件节点
        sb.AppendLine("    %% Domain Events");
        foreach (var domainEvent in analysisResult.DomainEvents)
        {
            var nodeId = GetNodeId(domainEvent.FullName, "DE");
            sb.AppendLine($"    {nodeId}(\"{EscapeMermaidText(domainEvent.Name)}\")");
        }
        sb.AppendLine();

        // 添加集成事件节点
        sb.AppendLine("    %% Integration Events");
        foreach (var integrationEvent in analysisResult.IntegrationEvents)
        {
            var nodeId = GetNodeId(integrationEvent.FullName, "IE");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(integrationEvent.Name)}\"]");
        }
        sb.AppendLine();

        // 添加事件处理器节点
        sb.AppendLine("    %% Event Handlers");
        foreach (var handler in analysisResult.DomainEventHandlers)
        {
            var nodeId = GetNodeId(handler.FullName, "DEH");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(handler.Name)}\"]");
        }
        
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            var nodeId = GetNodeId(handler.FullName, "IEH");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(handler.Name)}\"]");
        }
        sb.AppendLine();

        // 添加关系连接
        sb.AppendLine("    %% Relationships");
        foreach (var relationship in analysisResult.Relationships)
        {
            var sourceNodeId = FindNodeId(nodeIds, relationship.SourceType);
            var targetNodeId = FindNodeId(nodeIds, relationship.TargetType);
            
            if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
            {
                var arrow = GetArrowStyle(relationship.CallType);
                var label = GetRelationshipLabel(relationship.CallType);
                
                if (!string.IsNullOrEmpty(label))
                {
                    sb.AppendLine($"    {sourceNodeId} {arrow}|{label}| {targetNodeId}");
                }
                else
                {
                    sb.AppendLine($"    {sourceNodeId} {arrow} {targetNodeId}");
                }
            }
        }

        // 添加领域事件处理器到命令的关系
        foreach (var handler in analysisResult.DomainEventHandlers)
        {
            foreach (var commandType in handler.Commands)
            {
                var handlerNodeId = FindNodeId(nodeIds, handler.FullName);
                var commandNodeId = FindNodeId(nodeIds, commandType);
                
                if (!string.IsNullOrEmpty(handlerNodeId) && !string.IsNullOrEmpty(commandNodeId))
                {
                    var arrow = GetArrowStyle("HandlerToCommand");
                    var label = GetRelationshipLabel("HandlerToCommand");
                    
                    if (!string.IsNullOrEmpty(label))
                    {
                        sb.AppendLine($"    {handlerNodeId} {arrow}|{label}| {commandNodeId}");
                    }
                    else
                    {
                        sb.AppendLine($"    {handlerNodeId} {arrow} {commandNodeId}");
                    }
                }
            }
        }

        // 添加集成事件处理器到命令的关系
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            foreach (var commandType in handler.Commands)
            {
                var handlerNodeId = FindNodeId(nodeIds, handler.FullName);
                var commandNodeId = FindNodeId(nodeIds, commandType);
                
                if (!string.IsNullOrEmpty(handlerNodeId) && !string.IsNullOrEmpty(commandNodeId))
                {
                    var arrow = GetArrowStyle("HandlerToCommand");
                    var label = GetRelationshipLabel("HandlerToCommand");
                    
                    if (!string.IsNullOrEmpty(label))
                    {
                        sb.AppendLine($"    {handlerNodeId} {arrow}|{label}| {commandNodeId}");
                    }
                    else
                    {
                        sb.AppendLine($"    {handlerNodeId} {arrow} {commandNodeId}");
                    }
                }
            }
        }
        sb.AppendLine();

        // 添加样式
        AddStyles(sb);

        return sb.ToString();
    }

    /// <summary>
    /// 生成命令流程图（专注于命令执行流程）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>Mermaid 流程图字符串</returns>
    public static string GenerateCommandFlowChart(CodeFlowAnalysisResult analysisResult)
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
        var commandRelationships = analysisResult.Relationships
            .Where(r => r.CallType.Contains("Command") || r.CallType == "MethodToCommand")
            .ToList();

        var involvedTypes = new HashSet<string>();
        foreach (var rel in commandRelationships)
        {
            involvedTypes.Add(rel.SourceType);
            involvedTypes.Add(rel.TargetType);
        }

        // 添加相关的控制器
        foreach (var controller in analysisResult.Controllers.Where(c => involvedTypes.Contains(c.FullName)))
        {
            var nodeId = GetNodeId(controller.FullName, "C");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(controller.Name)}\"]");
        }

        // 添加相关的命令
        foreach (var command in analysisResult.Commands.Where(c => involvedTypes.Contains(c.FullName)))
        {
            var nodeId = GetNodeId(command.FullName, "CMD");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(command.Name)}\"]");
        }

        // 添加相关的实体
        foreach (var entity in analysisResult.Entities.Where(e => involvedTypes.Contains(e.FullName)))
        {
            var nodeId = GetNodeId(entity.FullName, "E");
            sb.AppendLine($"    {nodeId}{{{EscapeMermaidText(entity.Name)}}}");
        }

        sb.AppendLine();

        // 添加命令流程关系
        foreach (var relationship in commandRelationships)
        {
            var sourceNodeId = FindNodeId(nodeIds, relationship.SourceType);
            var targetNodeId = FindNodeId(nodeIds, relationship.TargetType);
            
            if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
            {
                var label = GetSimpleRelationshipLabel(relationship.CallType);
                sb.AppendLine($"    {sourceNodeId} --> |{label}| {targetNodeId}");
            }
        }

        sb.AppendLine();
        AddCommandFlowStyles(sb);

        return sb.ToString();
    }

    /// <summary>
    /// 生成事件流程图（专注于事件驱动流程）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>Mermaid 流程图字符串</returns>
    public static string GenerateEventFlowChart(CodeFlowAnalysisResult analysisResult)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");
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

        // 添加领域事件
        sb.AppendLine("    %% Domain Events");
        foreach (var domainEvent in analysisResult.DomainEvents)
        {
            var nodeId = GetNodeId(domainEvent.FullName, "DE");
            sb.AppendLine($"    {nodeId}(\"{EscapeMermaidText(domainEvent.Name)}\")");
        }
        sb.AppendLine();

        // 添加集成事件
        sb.AppendLine("    %% Integration Events");
        foreach (var integrationEvent in analysisResult.IntegrationEvents)
        {
            var nodeId = GetNodeId(integrationEvent.FullName, "IE");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(integrationEvent.Name)}\"]");
        }
        sb.AppendLine();

        // 添加事件处理器
        sb.AppendLine("    %% Event Handlers");
        foreach (var handler in analysisResult.DomainEventHandlers)
        {
            var nodeId = GetNodeId(handler.FullName, "DEH");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(handler.Name)}\"]");
        }
        
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            var nodeId = GetNodeId(handler.FullName, "IEH");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(handler.Name)}\"]");
        }
        sb.AppendLine();

        // 添加集成事件转换器
        sb.AppendLine("    %% Integration Event Converters");
        foreach (var converter in analysisResult.IntegrationEventConverters)
        {
            var nodeId = GetNodeId(converter.FullName, "IEC");
            sb.AppendLine($"    {nodeId}[/\"{EscapeMermaidText(converter.Name)}\"/]");
        }
        sb.AppendLine();

        // 添加事件相关关系
        var eventRelationships = analysisResult.Relationships
            .Where(r => r.CallType.Contains("Event") || r.CallType.Contains("Handler"))
            .ToList();

        foreach (var relationship in eventRelationships)
        {
            var sourceNodeId = FindNodeId(nodeIds, relationship.SourceType);
            var targetNodeId = FindNodeId(nodeIds, relationship.TargetType);
            
            if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
            {
                var arrow = GetEventArrowStyle(relationship.CallType);
                var label = GetEventRelationshipLabel(relationship.CallType);
                sb.AppendLine($"    {sourceNodeId} {arrow}|{label}| {targetNodeId}");
            }
        }

        sb.AppendLine();
        AddEventFlowStyles(sb);

        return sb.ToString();
    }

    /// <summary>
    /// 生成类图（展示类型间的关系）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>Mermaid 类图字符串</returns>
    public static string GenerateClassDiagram(CodeFlowAnalysisResult analysisResult)
    {
        var sb = new StringBuilder();
        sb.AppendLine("classDiagram");
        sb.AppendLine();

        // 添加控制器类
        foreach (var controller in analysisResult.Controllers)
        {
            var className = SanitizeClassName(controller.Name);
            sb.AppendLine($"    class {className} {{");
            sb.AppendLine("        <<Controller>>");
            foreach (var method in controller.Methods.Take(5))
            {
                sb.AppendLine($"        +{EscapeMermaidText(method)}()");
            }
            if (controller.Methods.Count > 5)
            {
                sb.AppendLine("        +...");
            }
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // 添加命令类
        foreach (var command in analysisResult.Commands)
        {
            var className = SanitizeClassName(command.Name);
            sb.AppendLine($"    class {className} {{");
            sb.AppendLine("        <<Command>>");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // 添加实体类
        foreach (var entity in analysisResult.Entities)
        {
            var className = SanitizeClassName(entity.Name);
            sb.AppendLine($"    class {className} {{");
            if (entity.IsAggregateRoot)
            {
                sb.AppendLine("        <<AggregateRoot>>");
            }
            else
            {
                sb.AppendLine("        <<Entity>>");
            }
            foreach (var method in entity.Methods.Take(5))
            {
                sb.AppendLine($"        +{EscapeMermaidText(method)}()");
            }
            if (entity.Methods.Count > 5)
            {
                sb.AppendLine("        +...");
            }
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // 添加关系
        foreach (var relationship in analysisResult.Relationships)
        {
            var sourceClass = GetClassNameFromFullName(relationship.SourceType);
            var targetClass = GetClassNameFromFullName(relationship.TargetType);
            
            if (!string.IsNullOrEmpty(sourceClass) && !string.IsNullOrEmpty(targetClass))
            {
                var relationshipType = GetClassDiagramRelationship(relationship.CallType);
                sb.AppendLine($"    {SanitizeClassName(sourceClass)} {relationshipType} {SanitizeClassName(targetClass)}");
            }
        }

        return sb.ToString();
    }

    #region 辅助方法

    private static string FindNodeId(Dictionary<string, string> nodeIds, string fullName)
    {
        return nodeIds.FirstOrDefault(kvp => kvp.Key.EndsWith(fullName)).Value ?? "";
    }

    private static string GetArrowStyle(string callType)
    {
        return callType switch
        {
            "MethodToCommand" => "-->",
            "CommandToAggregateMethod" => "==>",
            "DomainEventToHandler" => "-.->",
            "DomainEventToIntegrationEvent" => "===>",
            "IntegrationEventToHandler" => "-.->",
            "HandlerToCommand" => "-.->",
            _ => "-->"
        };
    }

    private static string GetEventArrowStyle(string callType)
    {
        return callType switch
        {
            "DomainEventToHandler" => "-.->",
            "DomainEventToIntegrationEvent" => "===>",
            "IntegrationEventToHandler" => "-.->",
            _ => "-->"
        };
    }

    private static string GetRelationshipLabel(string callType)
    {
        return callType switch
        {
            "MethodToCommand" => "sends",
            "CommandToAggregateMethod" => "executes",
            "DomainEventToHandler" => "handles",
            "DomainEventToIntegrationEvent" => "converts to",
            "IntegrationEventToHandler" => "subscribes",
            "HandlerToCommand" => "sends",
            _ => ""
        };
    }

    private static string GetSimpleRelationshipLabel(string callType)
    {
        return callType switch
        {
            "MethodToCommand" => "send",
            "CommandToAggregateMethod" => "execute",
            _ => "call"
        };
    }

    private static string GetEventRelationshipLabel(string callType)
    {
        return callType switch
        {
            "DomainEventToHandler" => "triggers",
            "DomainEventToIntegrationEvent" => "converts",
            "IntegrationEventToHandler" => "handles",
            _ => "processes"
        };
    }

    private static string GetClassDiagramRelationship(string callType)
    {
        return callType switch
        {
            "MethodToCommand" => "..>",
            "CommandToAggregateMethod" => "-->",
            "DomainEventToHandler" => "..>",
            "DomainEventToIntegrationEvent" => "-->",
            "IntegrationEventToHandler" => "..>",
            _ => "-->"
        };
    }

    private static string EscapeMermaidText(string text)
    {
        return text.Replace("\"", "&quot;").Replace("\n", " ").Replace("\r", "");
    }

    private static string SanitizeClassName(string className)
    {
        return className.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("[", "_").Replace("]", "_");
    }

    private static string GetClassNameFromFullName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        var parts = fullName.Split('.');
        return parts.LastOrDefault() ?? "";
    }

    private static void AddStyles(StringBuilder sb)
    {
        sb.AppendLine("    %% Styles");
        sb.AppendLine("    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:2px;");
        sb.AppendLine("    classDef command fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;");
        sb.AppendLine("    classDef entity fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px;");
        sb.AppendLine("    classDef domainEvent fill:#fff3e0,stroke:#e65100,stroke-width:2px;");
        sb.AppendLine("    classDef integrationEvent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef handler fill:#f1f8e9,stroke:#33691e,stroke-width:2px;");
        sb.AppendLine();
        
        sb.AppendLine("    class C1,C2,C3,C4,C5 controller;");
        sb.AppendLine("    class CMD1,CMD2,CMD3,CMD4,CMD5,CMD6,CMD7,CMD8,CMD9,CMD10 command;");
        sb.AppendLine("    class E1,E2,E3,E4,E5 entity;");
        sb.AppendLine("    class DE1,DE2,DE3,DE4,DE5 domainEvent;");
        sb.AppendLine("    class IE1,IE2,IE3,IE4,IE5 integrationEvent;");
        sb.AppendLine("    class DEH1,DEH2,DEH3,DEH4,DEH5,IEH1,IEH2,IEH3,IEH4,IEH5 handler;");
    }

    private static void AddCommandFlowStyles(StringBuilder sb)
    {
        sb.AppendLine("    %% Styles");
        sb.AppendLine("    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:2px;");
        sb.AppendLine("    classDef command fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;");
        sb.AppendLine("    classDef entity fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px;");
    }

    private static void AddEventFlowStyles(StringBuilder sb)
    {
        sb.AppendLine("    %% Styles");
        sb.AppendLine("    classDef domainEvent fill:#fff3e0,stroke:#e65100,stroke-width:2px;");
        sb.AppendLine("    classDef integrationEvent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef handler fill:#f1f8e9,stroke:#33691e,stroke-width:2px;");
        sb.AppendLine("    classDef converter fill:#e3f2fd,stroke:#0277bd,stroke-width:2px;");
        sb.AppendLine();
        
        sb.AppendLine("    class DE1,DE2,DE3,DE4,DE5 domainEvent;");
        sb.AppendLine("    class IE1,IE2,IE3,IE4,IE5 integrationEvent;");
        sb.AppendLine("    class DEH1,DEH2,DEH3,DEH4,DEH5,IEH1,IEH2,IEH3,IEH4,IEH5 handler;");
        sb.AppendLine("    class IEC1,IEC2,IEC3,IEC4,IEC5 converter;");
    }

    #endregion
}
