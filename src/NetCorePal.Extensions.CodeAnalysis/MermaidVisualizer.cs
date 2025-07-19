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
            sb.AppendLine($"    class {nodeId} controller;");
        }

        // 添加相关的命令发送者（除了已经作为控制器显示的）
        var controllerFullNames = new HashSet<string>(analysisResult.Controllers.Select(c => c.FullName));
        foreach (var sender in analysisResult.CommandSenders.Where(s => involvedTypes.Contains(s.FullName) && !controllerFullNames.Contains(s.FullName)))
        {
            var nodeId = GetNodeId(sender.FullName, "CS");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(sender.Name)}\"]");
            sb.AppendLine($"    class {nodeId} commandSender;");
        }

        // 添加相关的命令
        foreach (var command in analysisResult.Commands.Where(c => involvedTypes.Contains(c.FullName)))
        {
            var nodeId = GetNodeId(command.FullName, "CMD");
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(command.Name)}\"]");
            sb.AppendLine($"    class {nodeId} command;");
        }

        // 添加相关的实体
        foreach (var entity in analysisResult.Entities.Where(e => involvedTypes.Contains(e.FullName)))
        {
            var nodeId = GetNodeId(entity.FullName, "E");
            sb.AppendLine($"    {nodeId}{{{EscapeMermaidText(entity.Name)}}}");
            sb.AppendLine($"    class {nodeId} entity;");
        }

        sb.AppendLine();

        // 添加命令流程关系，去重每对节点之间的连线
        var processedLinks = new HashSet<string>();
        foreach (var relationship in commandRelationships)
        {
            var sourceNodeId = FindNodeId(nodeIds, relationship.SourceType);
            var targetNodeId = FindNodeId(nodeIds, relationship.TargetType);

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
    
    /// <summary>
    /// 生成类图（展示类型间的关系）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>Mermaid 类图字符串</returns>
    public static string GenerateClassDiagram(CodeFlowAnalysisResult analysisResult)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");
        sb.AppendLine();

        // 节点收集与样式
        var nodeTypes = new Dictionary<string, string>();

        // 控制器
        foreach (var controller in analysisResult.Controllers)
        {
            var nodeId = SanitizeClassName(controller.Name);
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(controller.Name)}\"]");
            sb.AppendLine($"    class {nodeId} controller;");
            nodeTypes[nodeId] = "controller";
        }

        // 命令发送者
        var controllerFullNames = new HashSet<string>(analysisResult.Controllers.Select(c => c.FullName));
        foreach (var sender in analysisResult.CommandSenders.Where(s => !controllerFullNames.Contains(s.FullName)))
        {
            var nodeId = SanitizeClassName(sender.Name);
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(sender.Name)}\"]");
            sb.AppendLine($"    class {nodeId} commandSender;");
            nodeTypes[nodeId] = "commandSender";
        }

        // 命令
        foreach (var command in analysisResult.Commands)
        {
            var nodeId = SanitizeClassName(command.Name);
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(command.Name)}\"]");
            sb.AppendLine($"    class {nodeId} command;");
            nodeTypes[nodeId] = "command";
        }

        // 实体
        foreach (var entity in analysisResult.Entities)
        {
            var nodeId = SanitizeClassName(entity.Name);
            var shape = entity.IsAggregateRoot ? "{{" + EscapeMermaidText(entity.Name) + "}}" : "[" + EscapeMermaidText(entity.Name) + "]";
            sb.AppendLine($"    {nodeId}{shape}");
            sb.AppendLine($"    class {nodeId} entity;");
            nodeTypes[nodeId] = "entity";
        }

        // 连线（去重）
        var processedLinks = new HashSet<string>();
        var relationshipPriority = new Dictionary<string, int>
        {
            { "CommandToAggregateMethod", 1 },
            { "MethodToCommand", 2 },
            { "DomainEventToHandler", 3 },
            { "IntegrationEventToHandler", 4 },
            { "DomainEventToIntegrationEvent", 5 },
            { "HandlerToCommand", 6 }
        };
        var sortedRelationships = analysisResult.Relationships
            .OrderBy(r => relationshipPriority.TryGetValue(r.CallType, out var priority) ? priority : 999)
            .ToList();

        foreach (var relationship in sortedRelationships)
        {
            var sourceId = SanitizeClassName(GetClassNameFromFullName(relationship.SourceType));
            var targetId = SanitizeClassName(GetClassNameFromFullName(relationship.TargetType));
            var linkKey = $"{sourceId}->{targetId}";
            if (!string.IsNullOrEmpty(sourceId) && !string.IsNullOrEmpty(targetId) && !processedLinks.Contains(linkKey))
            {
                processedLinks.Add(linkKey);
                var arrow = GetArrowStyle(relationship.CallType);
                var label = GetRelationshipLabel(relationship.CallType, relationship.SourceMethod, relationship.TargetMethod);
                if (!string.IsNullOrEmpty(label))
                    sb.AppendLine($"    {sourceId} {arrow}|{label}| {targetId}");
                else
                    sb.AppendLine($"    {sourceId} {arrow} {targetId}");
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

    
    #region 辅助方法
    /// <summary>
    /// 生成聚合关系图（以指定聚合为核心，遇到其它聚合则作为结束节点）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <param name="aggregateFullName">核心聚合的 FullName</param>
    /// <returns>Mermaid 图字符串</returns>
    public static string GenerateAggregateRelationDiagram(CodeFlowAnalysisResult analysisResult, string aggregateFullName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");
        sb.AppendLine();

        var nodeIds = new Dictionary<string, string>();
        var nodeIdCounter = 1;
        string GetNodeId(string fullName)
        {
            return SanitizeClassName(GetClassNameFromFullName(fullName));
        }

        var processedNodes = new HashSet<string>();
        var processedLinks = new HashSet<string>();

        // 只收集所有"链路上包含主聚合"的节点和连线
        void Traverse(string currentType, HashSet<string> path, bool onMainAggregatePath)
        {
            if (path.Contains(currentType)) return;
            path.Add(currentType);

            var entity = analysisResult.Entities.FirstOrDefault(e => e.FullName == currentType);
            bool isMainAggregate = (currentType == aggregateFullName);
            bool isOtherAggregate = entity != null && entity.IsAggregateRoot && !isMainAggregate;

            // 如果当前在主聚合路径上，或者当前就是主聚合，则收集节点
            bool shouldCollect = onMainAggregatePath || isMainAggregate;

            string nodeId = GetNodeId(currentType);
            string nodeLabel = entity != null ? entity.Name : GetClassNameFromFullName(currentType);

            // 收集节点
            if (shouldCollect && !processedNodes.Contains(currentType))
            {
                if (entity != null && entity.IsAggregateRoot)
                {
                    sb.AppendLine($"    {nodeId}{{{EscapeMermaidText(nodeLabel)}}}");
                    sb.AppendLine($"    class {nodeId} entity;");
                }
                else if (entity != null)
                {
                    sb.AppendLine($"    {nodeId}[{EscapeMermaidText(nodeLabel)}]");
                    sb.AppendLine($"    class {nodeId} entity;");
                }
                else
                {
                    var controller = analysisResult.Controllers.FirstOrDefault(c => c.FullName == currentType);
                    if (controller != null)
                    {
                        sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(controller.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} controller;");
                    }
                    var command = analysisResult.Commands.FirstOrDefault(c => c.FullName == currentType);
                    if (command != null)
                    {
                        sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(command.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} command;");
                    }
                    var domainEvent = analysisResult.DomainEvents.FirstOrDefault(d => d.FullName == currentType);
                    if (domainEvent != null)
                    {
                        sb.AppendLine($"    {nodeId}(\"{EscapeMermaidText(domainEvent.Name)}\")");
                        sb.AppendLine($"    class {nodeId} domainEvent;");
                    }
                    var integrationEvent = analysisResult.IntegrationEvents.FirstOrDefault(i => i.FullName == currentType);
                    if (integrationEvent != null)
                    {
                        sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(integrationEvent.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} integrationEvent;");
                    }
                    var domainEventHandler = analysisResult.DomainEventHandlers.FirstOrDefault(h => h.FullName == currentType);
                    if (domainEventHandler != null)
                    {
                        sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(GetClassNameFromFullName(domainEventHandler.FullName))}\"]");
                        sb.AppendLine($"    class {nodeId} handler;");
                    }
                    var integrationEventHandler = analysisResult.IntegrationEventHandlers.FirstOrDefault(h => h.FullName == currentType);
                    if (integrationEventHandler != null)
                    {
                        sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(GetClassNameFromFullName(integrationEventHandler.FullName))}\"]");
                        sb.AppendLine($"    class {nodeId} handler;");
                    }
                }
                processedNodes.Add(currentType);
            }

            // 递归下游
            foreach (var rel in analysisResult.Relationships.Where(r => r.SourceType == currentType))
            {
                var targetEntity = analysisResult.Entities.FirstOrDefault(e => e.FullName == rel.TargetType);
                bool targetIsOtherAggregate = targetEntity != null && targetEntity.IsAggregateRoot && rel.TargetType != aggregateFullName;
                string targetNodeId = GetNodeId(rel.TargetType);

                // 收集连线
                if (shouldCollect)
                {
                    var linkKey = $"{nodeId}->{targetNodeId}";
                    if (!processedLinks.Contains(linkKey))
                    {
                        processedLinks.Add(linkKey);
                        if (!string.IsNullOrEmpty(rel.CallType))
                            sb.AppendLine($"    {nodeId} -->|{EscapeMermaidText(rel.CallType)}| {targetNodeId}");
                        else
                            sb.AppendLine($"    {nodeId} --> {targetNodeId}");
                    }
                }

                // 如果目标是非主聚合，不递归其下游
                if (targetIsOtherAggregate) continue;

                // 递归下游：如果当前在主聚合路径上，或者当前是主聚合，则下游也在主聚合路径上
                bool nextOnMainPath = onMainAggregatePath || isMainAggregate;
                Traverse(rel.TargetType, path, nextOnMainPath);
            }

            // 递归上游
            foreach (var rel in analysisResult.Relationships.Where(r => r.TargetType == currentType))
            {
                var sourceEntity = analysisResult.Entities.FirstOrDefault(e => e.FullName == rel.SourceType);
                bool sourceIsOtherAggregate = sourceEntity != null && sourceEntity.IsAggregateRoot && rel.SourceType != aggregateFullName;
                string sourceNodeId = GetNodeId(rel.SourceType);

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
                        if (!string.IsNullOrEmpty(rel.CallType))
                            sb.AppendLine($"    {sourceNodeId} -->|{EscapeMermaidText(rel.CallType)}| {nodeId}");
                        else
                            sb.AppendLine($"    {sourceNodeId} --> {nodeId}");
                    }
                }

                // 如果源是非主聚合，不递归其上游
                if (sourceIsOtherAggregate) continue;

                // 递归上游：如果当前在主聚合路径上，或者当前是主聚合，则上游也在主聚合路径上
                bool nextOnMainPath = onMainAggregatePath || isMainAggregate;
                Traverse(rel.SourceType, path, nextOnMainPath);
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
    
    private static string GetRelationshipLabel(string callType, string sourceMethod = "", string targetMethod = "")
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
        return text.Replace("\"", "&quot;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("\n", " ")
                   .Replace("\r", "");
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
    
    /// <summary>
    /// 生成所有独立链路流程图的集合
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>包含所有独立链路图的元组列表，每个链路对应一张图</returns>
    public static List<(string ChainName, string Diagram)> GenerateAllChainFlowCharts(CodeFlowAnalysisResult analysisResult)
    {
        var chainGroups = GenerateMultiChainGroups(analysisResult);
        var chainFlowCharts = new List<(string ChainName, string Diagram)>();

        foreach (var (chainName, chainNodes, chainRelations, chainNodeIds) in chainGroups)
        {
            var sb = new StringBuilder();
            sb.AppendLine("flowchart TD");
            sb.AppendLine();
            sb.AppendLine($"    %% {EscapeMermaidText(chainName)}");
            sb.AppendLine();

            // 收集节点样式映射
            var nodeStyleMap = new Dictionary<string, string>();

            // 添加该链路的所有节点
            foreach (var nodeFullName in chainNodes)
            {
                var nodeId = chainNodeIds[nodeFullName];
                AddMultiChainNodeSimple(sb, nodeFullName, nodeId, analysisResult, "    ");

                // 记录节点样式映射
                var nodeStyleClass = GetNodeStyleClass(nodeFullName, analysisResult);
                if (!string.IsNullOrEmpty(nodeStyleClass))
                {
                    nodeStyleMap[nodeId] = nodeStyleClass;
                }
            }

            sb.AppendLine();
            sb.AppendLine("    %% Chain Relationships");

            // 添加该链路的关系
            foreach (var (source, target, label) in chainRelations)
            {
                var sourceNodeId = chainNodeIds.TryGetValue(source, out var srcId) ? srcId : string.Empty;
                var targetNodeId = chainNodeIds.TryGetValue(target, out var tgtId) ? tgtId : string.Empty;

                if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
                {
                    var arrow = GetArrowStyle("Default");
                    if (!string.IsNullOrEmpty(label))
                    {
                        sb.AppendLine($"    {sourceNodeId} {arrow}|{EscapeMermaidText(label)}| {targetNodeId}");
                    }
                    else
                    {
                        sb.AppendLine($"    {sourceNodeId} {arrow} {targetNodeId}");
                    }
                }
            }

            sb.AppendLine();
            AddMultiChainStyles(sb, nodeStyleMap);

            chainFlowCharts.Add((chainName, sb.ToString()));
        }

        return chainFlowCharts;
    }

    /// <summary>
    /// chainGroups
    /// </summary>
    private static List<(string ChainName, List<string> ChainNodes, List<(string Source, string Target, string Label)> ChainRelations, Dictionary<string, string> ChainNodeIds)> GenerateMultiChainGroups(CodeFlowAnalysisResult analysisResult)
    {
        var globalNodeCounter = 1;
        var chainGroups = new List<(string ChainName, List<string> ChainNodes, List<(string Source, string Target, string Label)> ChainRelations, Dictionary<string, string> ChainNodeIds)>();

        // 找出所有潜在的链路起点（没有上游关系的节点）
        var allUpstreamTargets = new HashSet<string>();

        // 收集所有作为目标的节点（有上游关系的节点），但排除从Controller方法发出的命令
        foreach (var rel in analysisResult.Relationships)
        {
            // 对于方法级别的关系，我们需要记录具体的方法节点
            if (rel.CallType == "CommandToAggregateMethod")
            {
                // 聚合方法有上游关系
                allUpstreamTargets.Add($"{rel.TargetType}::{rel.TargetMethod}");
            }
            else if (rel.CallType.Contains("EventToHandler"))
            {
                allUpstreamTargets.Add(rel.TargetType); // 事件处理器有上游关系
                // 也将事件处理器的方法节点标记为有上游关系
                if (rel.CallType == "DomainEventToHandler")
                {
                    allUpstreamTargets.Add($"{rel.TargetType}::Handle");
                }
                else if (rel.CallType == "IntegrationEventToHandler")
                {
                    allUpstreamTargets.Add($"{rel.TargetType}::HandleAsync");
                }
            }
            else if (rel.CallType.Contains("ToIntegrationEvent"))
            {
                allUpstreamTargets.Add(rel.TargetType); // 集成事件有上游关系
            }
            else if (rel.CallType.Contains("ToDomainEvent"))
            {
                allUpstreamTargets.Add(rel.TargetType); // 领域事件有上游关系
            }
            // 注意：我们不将MethodToCommand的目标加入上游关系，这样所有Controller方法都可以作为起点
        }

        // 删除不再需要的事件处理器命令标记逻辑，让Controller方法优先作为起点

        int chainIndex = 1;

        // 找出所有链路起点并构建链路
        var potentialStarts = new List<string>();

        // 1. Controller 方法作为起点
        foreach (var controller in analysisResult.Controllers)
        {
            var controllerMethodRelations = analysisResult.Relationships
                .Where(r => r.SourceType == controller.FullName && r.CallType == "MethodToCommand")
                .ToList();

            foreach (var methodRel in controllerMethodRelations)
            {
                var methodNode = $"{controller.FullName}::{methodRel.SourceMethod}";
                if (!allUpstreamTargets.Contains(methodNode))
                {
                    potentialStarts.Add(methodNode);
                }
            }
        }

        // 2. 领域事件处理器作为起点（只有那些处理的事件没有上游发布者的）
        foreach (var handler in analysisResult.DomainEventHandlers)
        {
            var handlerMethodNode = $"{handler.FullName}::Handle";
            // 检查处理器本身和其处理的事件类型是否都没有上游
            if (!allUpstreamTargets.Contains(handlerMethodNode) && 
                !allUpstreamTargets.Contains(handler.FullName) &&
                !allUpstreamTargets.Contains(handler.HandledEventType))
            {
                potentialStarts.Add(handlerMethodNode);
            }
        }

        // 3. 集成事件处理器作为起点（只有那些处理的事件没有转换器的）
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            var handlerMethodNode = $"{handler.FullName}::HandleAsync";
            // 检查其处理的集成事件是否有转换器（即是否有上游来源）
            var hasConverter = analysisResult.IntegrationEventConverters.Any(c => 
                c.IntegrationEventType == handler.HandledEventType);
            
            if (!hasConverter)
            {
                potentialStarts.Add(handlerMethodNode);
            }
        }

        // 4. 其他命令发送者作为起点（除了已经作为控制器处理的）
        var controllerFullNames = new HashSet<string>(analysisResult.Controllers.Select(c => c.FullName));
        foreach (var sender in analysisResult.CommandSenders.Where(s => !controllerFullNames.Contains(s.FullName)))
        {
            foreach (var method in sender.Methods)
            {
                var senderMethodNode = $"{sender.FullName}::{method}";
                if (!allUpstreamTargets.Contains(senderMethodNode))
                {
                    potentialStarts.Add(senderMethodNode);
                }
            }
        }

        // 为每个起点构建链路
        foreach (var startNode in potentialStarts)
        {
            var chainNodes = new List<string>();
            var chainRelations = new List<(string Source, string Target, string Label)>();
            var localProcessedNodes = new HashSet<string>(); // 本链路内已处理的节点
            var chainNodeIds = new Dictionary<string, string>(); // 每个链路独立的节点ID映射

            // 构建链路（移除全局节点重复检查，每个链路独立完整）
            BuildSingleChain(analysisResult, startNode, chainNodes, chainRelations, localProcessedNodes);

            if (chainNodes.Count > 0)
            {
                // 为链路中的每个节点分配独立的ID
                foreach (var nodeFullName in chainNodes)
                {
                    chainNodeIds[nodeFullName] = $"N{globalNodeCounter++}";
                }

                var startNodeName = GetSimpleNodeName(startNode);
                var chainName = startNodeName;
                chainGroups.Add((chainName, chainNodes, chainRelations, chainNodeIds));
                chainIndex++;
            }
        }
        return chainGroups;
    }

    /// <summary>
    /// 构建单个链路
    /// </summary>
    private static void BuildSingleChain(CodeFlowAnalysisResult analysisResult, string startNode,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> localProcessedNodes)
    {
        if (localProcessedNodes.Contains(startNode))
            return;

        // 添加起始节点
        chainNodes.Add(startNode);
        localProcessedNodes.Add(startNode);

        var (nodeType, nodeMethod) = ParseNodeName(startNode);

        // 查找从这个节点类型出发的关系
        var outgoingRelations = analysisResult.Relationships
            .Where(r => r.SourceType == nodeType)
            .ToList();

        foreach (var relation in outgoingRelations)
        {
            if (relation.CallType == "MethodToCommand" &&
                (string.IsNullOrEmpty(nodeMethod) || relation.SourceMethod == nodeMethod))
            {
                // 添加从Controller方法到Command的关系
                if (!localProcessedNodes.Contains(relation.TargetType))
                {
                    BuildChainFromCommand(analysisResult, relation.TargetType, startNode, chainNodes, chainRelations, localProcessedNodes);
                }
            }
        }

        // 对于事件处理器，追踪其发出的命令
        var domainHandler = analysisResult.DomainEventHandlers.FirstOrDefault(h => h.FullName == nodeType);
        var integrationHandler = analysisResult.IntegrationEventHandlers.FirstOrDefault(h => h.FullName == nodeType);

        var commands = domainHandler?.Commands ?? integrationHandler?.Commands;
        if (commands != null)
        {
            foreach (var commandType in commands)
            {
                if (!localProcessedNodes.Contains(commandType))
                {
                    BuildChainFromCommand(analysisResult, commandType, startNode, chainNodes, chainRelations, localProcessedNodes);
                }
            }
        }
    }

    /// <summary>
    /// 从命令开始构建链路
    /// </summary>
    private static void BuildChainFromCommand(CodeFlowAnalysisResult analysisResult, string commandType, string sourceNode,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> localProcessedNodes)
    {
        if (localProcessedNodes.Contains(commandType))
            return;

        // 添加命令节点
        chainNodes.Add(commandType);
        localProcessedNodes.Add(commandType);
        chainRelations.Add((sourceNode, commandType, "sends"));

        // 查找命令执行的聚合/实体方法
        var commandRelations = analysisResult.Relationships
            .Where(r => r.SourceType == commandType && r.CallType == "CommandToAggregateMethod")
            .ToList();

        foreach (var relation in commandRelations)
        {
            var targetWithMethod = $"{relation.TargetType}::{relation.TargetMethod}";

            if (!localProcessedNodes.Contains(targetWithMethod))
            {
                BuildChainFromAggregateMethod(analysisResult, relation.TargetType, relation.TargetMethod,
                    targetWithMethod, commandType, chainNodes, chainRelations, localProcessedNodes);
            }
        }
    }

    /// <summary>
    /// 从聚合方法构建链路
    /// </summary>
    private static void BuildChainFromAggregateMethod(CodeFlowAnalysisResult analysisResult, string aggregateType,
        string aggregateMethod, string aggregateMethodNode, string sourceNode,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> localProcessedNodes)
    {
        if (localProcessedNodes.Contains(aggregateMethodNode))
            return;

        // 添加聚合方法节点
        chainNodes.Add(aggregateMethodNode);
        localProcessedNodes.Add(aggregateMethodNode);
        chainRelations.Add((sourceNode, aggregateMethodNode, "execute"));

        // 查找从聚合方法产生的领域事件
        var domainEventRelations = analysisResult.Relationships
            .Where(r => r.SourceType == aggregateType && r.SourceMethod == aggregateMethod && r.CallType == "MethodToDomainEvent")
            .ToList();

        foreach (var eventRelation in domainEventRelations)
        {
            var eventType = eventRelation.TargetType;

            if (!localProcessedNodes.Contains(eventType))
            {
                BuildChainFromDomainEvent(analysisResult, eventType, aggregateMethodNode, chainNodes, chainRelations, localProcessedNodes);
            }
            else
            {
                // 即使事件已经存在，也要添加从当前聚合方法到事件的关系
                chainRelations.Add((aggregateMethodNode, eventType, "publishes"));
            }
        }
    }

    /// <summary>
    /// 从领域事件构建链路
    /// </summary>
    private static void BuildChainFromDomainEvent(CodeFlowAnalysisResult analysisResult, string eventType, string sourceNode,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> localProcessedNodes)
    {
        if (localProcessedNodes.Contains(eventType))
            return;

        // 添加领域事件节点
        chainNodes.Add(eventType);
        localProcessedNodes.Add(eventType);
        chainRelations.Add((sourceNode, eventType, "publishes"));

        // 查找处理该事件的处理器
        var handlers = analysisResult.DomainEventHandlers
            .Where(h => h.HandledEventType == eventType)
            .ToList();

        foreach (var handler in handlers)
        {
            if (!localProcessedNodes.Contains(handler.FullName))
            {
                chainNodes.Add(handler.FullName);
                localProcessedNodes.Add(handler.FullName);
                chainRelations.Add((eventType, handler.FullName, "handles"));

                // 跟踪处理器发出的命令
                foreach (var commandType in handler.Commands)
                {
                    if (!localProcessedNodes.Contains(commandType))
                    {
                        // 命令还没有处理过，完整构建链路
                        BuildChainFromCommand(analysisResult, commandType, handler.FullName, chainNodes, chainRelations, localProcessedNodes);
                    }
                    else
                    {
                        // 命令已经处理过，只添加关系，不再递归
                        chainRelations.Add((handler.FullName, commandType, "sends"));
                    }
                }
            }
        }

        // 查找领域事件到集成事件的转换
        var converters = analysisResult.IntegrationEventConverters
            .Where(c => c.DomainEventType == eventType)
            .ToList();

        foreach (var converter in converters)
        {
            if (!localProcessedNodes.Contains(converter.FullName))
            {
                chainNodes.Add(converter.FullName);
                localProcessedNodes.Add(converter.FullName);
                chainRelations.Add((eventType, converter.FullName, "converts"));

                // 追踪转换后的集成事件
                var integrationEventType = converter.IntegrationEventType;
                if (!localProcessedNodes.Contains(integrationEventType))
                {
                    BuildChainFromIntegrationEvent(analysisResult, integrationEventType, converter.FullName, chainNodes, chainRelations, localProcessedNodes);
                }
            }
        }
    }

    /// <summary>
    /// 从集成事件构建链路
    /// </summary>
    private static void BuildChainFromIntegrationEvent(CodeFlowAnalysisResult analysisResult, string integrationEventType, string sourceNode,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> localProcessedNodes)
    {
        if (localProcessedNodes.Contains(integrationEventType))
            return;

        // 添加集成事件节点
        chainNodes.Add(integrationEventType);
        localProcessedNodes.Add(integrationEventType);
        chainRelations.Add((sourceNode, integrationEventType, "to"));

        // 追踪集成事件处理器
        var integrationHandlers = analysisResult.IntegrationEventHandlers
            .Where(h => h.HandledEventType == integrationEventType)
            .ToList();

        foreach (var integrationHandler in integrationHandlers)
        {
            if (!localProcessedNodes.Contains(integrationHandler.FullName))
            {
                chainNodes.Add(integrationHandler.FullName);
                localProcessedNodes.Add(integrationHandler.FullName);
                chainRelations.Add((integrationEventType, integrationHandler.FullName, "handles"));

                // 跟踪集成事件处理器发出的命令
                foreach (var commandType in integrationHandler.Commands)
                {
                    if (!localProcessedNodes.Contains(commandType))
                    {
                        // 命令还没有处理过，完整构建链路
                        BuildChainFromCommand(analysisResult, commandType, integrationHandler.FullName, chainNodes, chainRelations, localProcessedNodes);
                    }
                    else
                    {
                        // 命令已经处理过，只添加关系，不再递归
                        chainRelations.Add((integrationHandler.FullName, commandType, "sends"));
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 跟踪链路执行
    /// </summary>
    private static void TraceChainExecution(CodeFlowAnalysisResult analysisResult, string commandType,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> visitedInChain)
    {
        var commandRelations = analysisResult.Relationships
            .Where(r => r.SourceType == commandType)
            .ToList();

        foreach (var relation in commandRelations)
        {
            var targetType = relation.TargetType;

            if (!visitedInChain.Contains(targetType))
            {
                chainNodes.Add(targetType);
                visitedInChain.Add(targetType);

                var label = GetRelationshipLabel(relation.CallType, relation.SourceMethod, relation.TargetMethod);
                chainRelations.Add((relation.SourceType, targetType, label));

                // 如果目标是聚合根，继续跟踪其产生的领域事件
                var targetEntity = analysisResult.Entities.FirstOrDefault(e => e.FullName == targetType);
                if (targetEntity != null && targetEntity.IsAggregateRoot)
                {
                    TraceDomainEventsInChain(analysisResult, targetType, chainNodes, chainRelations, visitedInChain);
                }
            }
        }
    }

    /// <summary>
    /// 跟踪链路中的领域事件
    /// </summary>
    private static void TraceDomainEventsInChain(CodeFlowAnalysisResult analysisResult, string aggregateType,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> visitedInChain)
    {
        // 查找从聚合根产生的领域事件
        var domainEventRelations = analysisResult.Relationships
            .Where(r => r.SourceType == aggregateType && r.CallType.Contains("Event"))
            .ToList();

        foreach (var relation in domainEventRelations)
        {
            var eventType = relation.TargetType;

            if (!visitedInChain.Contains(eventType))
            {
                chainNodes.Add(eventType);
                visitedInChain.Add(eventType);

                var label = GetRelationshipLabel(relation.CallType, relation.SourceMethod, relation.TargetMethod);
                chainRelations.Add((aggregateType, eventType, label));

                // 跟踪事件处理器
                TraceEventHandlersInChain(analysisResult, eventType, chainNodes, chainRelations, visitedInChain);
            }
        }
    }

    /// <summary>
    /// 跟踪链路中的事件处理器
    /// </summary>
    private static void TraceEventHandlersInChain(CodeFlowAnalysisResult analysisResult, string eventType,
        List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations,
        HashSet<string> visitedInChain)
    {
        // 查找处理该事件的处理器
        var handlers = analysisResult.DomainEventHandlers
            .Where(h => h.HandledEventType == eventType)
            .ToList();

        foreach (var handler in handlers)
        {
            if (!visitedInChain.Contains(handler.FullName))
            {
                chainNodes.Add(handler.FullName);
                visitedInChain.Add(handler.FullName);

                chainRelations.Add((eventType, handler.FullName, "handles"));

                // 跟踪处理器发出的命令
                foreach (var commandType in handler.Commands)
                {
                    if (!visitedInChain.Contains(commandType))
                    {
                        chainNodes.Add(commandType);
                        visitedInChain.Add(commandType);

                        chainRelations.Add((handler.FullName, commandType, "sends"));

                        // 递归跟踪命令执行
                        TraceChainExecution(analysisResult, commandType, chainNodes, chainRelations, visitedInChain);
                    }
                }
            }
        }

        // 查找集成事件转换器
        var converters = analysisResult.IntegrationEventConverters
            .Where(c => c.DomainEventType == eventType)
            .ToList();

        foreach (var converter in converters)
        {
            if (!visitedInChain.Contains(converter.FullName))
            {
                chainNodes.Add(converter.FullName);
                visitedInChain.Add(converter.FullName);

                chainRelations.Add((eventType, converter.FullName, "converts"));

                // 追踪转换后的集成事件
                var integrationEventType = converter.IntegrationEventType;
                if (!visitedInChain.Contains(integrationEventType))
                {
                    chainNodes.Add(integrationEventType);
                    visitedInChain.Add(integrationEventType);

                    chainRelations.Add((converter.FullName, integrationEventType, "to"));

                    // 追踪集成事件处理器
                    var integrationHandlers = analysisResult.IntegrationEventHandlers
                        .Where(h => h.HandledEventType == integrationEventType)
                        .ToList();

                    foreach (var integrationHandler in integrationHandlers)
                    {
                        if (!visitedInChain.Contains(integrationHandler.FullName))
                        {
                            chainNodes.Add(integrationHandler.FullName);
                            visitedInChain.Add(integrationHandler.FullName);

                            chainRelations.Add((integrationEventType, integrationHandler.FullName, "handles"));

                            // 跟踪集成事件处理器发出的命令
                            foreach (var commandType in integrationHandler.Commands)
                            {
                                if (!visitedInChain.Contains(commandType))
                                {
                                    chainNodes.Add(commandType);
                                    visitedInChain.Add(commandType);

                                    chainRelations.Add((integrationHandler.FullName, commandType, "sends"));

                                    // 递归跟踪命令执行
                                    TraceChainExecution(analysisResult, commandType, chainNodes, chainRelations, visitedInChain);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 添加多链路图样式
    /// </summary>
    /// <summary>
    /// 添加多链路图样式
    /// </summary>
    private static void AddMultiChainStyles(StringBuilder sb, Dictionary<string, string>? nodeStyleMap = null)
    {
        sb.AppendLine("    %% Multi-Chain Styles");
        sb.AppendLine("    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:2px;");
        sb.AppendLine("    classDef commandSender fill:#fff8e1,stroke:#f57f17,stroke-width:2px;");
        sb.AppendLine("    classDef command fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;");
        sb.AppendLine("    classDef entity fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px;");
        sb.AppendLine("    classDef domainEvent fill:#fff3e0,stroke:#e65100,stroke-width:2px;");
        sb.AppendLine("    classDef integrationEvent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef handler fill:#f1f8e9,stroke:#33691e,stroke-width:2px;");
        sb.AppendLine("    classDef converter fill:#e3f2fd,stroke:#0277bd,stroke-width:2px;");
        sb.AppendLine();

        // 应用样式到具体节点
        if (nodeStyleMap != null && nodeStyleMap.Count > 0)
        {
            sb.AppendLine("    %% Apply styles to specific nodes");

            // 按样式类分组节点
            var nodesByStyle = nodeStyleMap.GroupBy(kvp => kvp.Value);

            foreach (var styleGroup in nodesByStyle)
            {
                var styleClass = styleGroup.Key;
                var nodeIds = styleGroup.Select(kvp => kvp.Key).ToList();

                if (nodeIds.Count > 0)
                {
                    var nodeIdList = string.Join(",", nodeIds);
                    sb.AppendLine($"    class {nodeIdList} {styleClass};");
                }
            }
        }
        else
        {
            // 旧的固定格式支持（向后兼容）
            sb.AppendLine("    %% Apply styles to node types");
            for (int i = 1; i <= 50; i++) // 支持最多50个节点
            {
                sb.AppendLine($"    class C{i} controller;");
                sb.AppendLine($"    class CMD{i} command;");
                sb.AppendLine($"    class E{i},N{i} entity;");
                sb.AppendLine($"    class DE{i} domainEvent;");
                sb.AppendLine($"    class IE{i} integrationEvent;");
                sb.AppendLine($"    class DEH{i},IEH{i} handler;");
                sb.AppendLine($"    class IEC{i} converter;");
            }
        }
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
    /// 获取简单的节点显示名称
    /// </summary>
    private static string GetSimpleNodeName(string nodeFullName)
    {
        var (nodeType, method) = ParseNodeName(nodeFullName);
        var className = GetClassNameFromFullName(nodeType);

        if (!string.IsNullOrEmpty(method))
        {
            return $"{className}.{method}";
        }
        return className;
    }

    /// <summary>
    /// 添加简单的多链路图节点
    /// </summary>
    private static void AddMultiChainNodeSimple(StringBuilder sb, string nodeFullName, string nodeId,
        CodeFlowAnalysisResult analysisResult, string indent)
    {
        var (nodeType, method) = ParseNodeName(nodeFullName);
        var displayName = GetSimpleNodeName(nodeFullName);

        // 根据节点类型确定样式
        var controller = analysisResult.Controllers.FirstOrDefault(c => c.FullName == nodeType);
        var commandSender = analysisResult.CommandSenders.FirstOrDefault(cs => cs.FullName == nodeType);
        var command = analysisResult.Commands.FirstOrDefault(c => c.FullName == nodeType);
        var entity = analysisResult.Entities.FirstOrDefault(e => e.FullName == nodeType);
        var domainEvent = analysisResult.DomainEvents.FirstOrDefault(d => d.FullName == nodeType);
        var integrationEvent = analysisResult.IntegrationEvents.FirstOrDefault(i => i.FullName == nodeType);
        var domainEventHandler = analysisResult.DomainEventHandlers.FirstOrDefault(h => h.FullName == nodeType);
        var integrationEventHandler = analysisResult.IntegrationEventHandlers.FirstOrDefault(h => h.FullName == nodeType);

        if (controller != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(displayName)}\"]");
        }
        else if (commandSender != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(displayName)}\"]");
        }
        else if (command != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(displayName)}\"]");
        }
        else if (entity != null)
        {
            var shape = entity.IsAggregateRoot ? "{{" + EscapeMermaidText(displayName) + "}}" : "[" + EscapeMermaidText(displayName) + "]";
            sb.AppendLine($"{indent}{nodeId}{shape}");
        }
        else if (domainEvent != null)
        {
            sb.AppendLine($"{indent}{nodeId}(\"{EscapeMermaidText(displayName)}\")");
        }
        else if (integrationEvent != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(displayName)}\"]");
        }
        else if (domainEventHandler != null || integrationEventHandler != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(displayName)}\"]");
        }
        else
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(displayName)}\"]");
        }
    }

    /// <summary>
    /// 生成完整的可视化HTML页面
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <param name="title">页面标题</param>
    /// <returns>完整的HTML页面内容</returns>
    public static string GenerateVisualizationHtml(CodeFlowAnalysisResult analysisResult, string title = "NetCorePal 架构图可视化")
    {
        var sb = new StringBuilder();

        // 生成所有类型的图表
        var commandFlowChart = GenerateCommandFlowChart(analysisResult);
        var classDiagram = GenerateClassDiagram(analysisResult);
        var allChainFlowCharts = GenerateAllChainFlowCharts(analysisResult);
        var allAggregateRelationDiagrams = GenerateAllAggregateRelationDiagrams(analysisResult);

        // 生成HTML结构
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"zh-CN\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>{EscapeHtml(title)}</title>");
        sb.AppendLine("    <script src=\"https://unpkg.com/mermaid@10.6.1/dist/mermaid.min.js\"></script>");
        sb.AppendLine("    <script src=\"https://unpkg.com/pako@2.1.0/dist/pako.min.js\"></script>");

        // 添加CSS样式
        AddHtmlStyles(sb);

        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // 添加页面结构
        AddHtmlStructureWithAggregate(sb, allAggregateRelationDiagrams.Count);

        // 添加JavaScript逻辑
        AddHtmlScriptWithAggregate(sb, analysisResult, commandFlowChart, classDiagram, allChainFlowCharts, allAggregateRelationDiagrams);

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// 添加HTML样式
    /// </summary>
    private static void AddHtmlStyles(StringBuilder sb)
    {
        sb.AppendLine("    <style>");
        sb.AppendLine("        * {");
        sb.AppendLine("            margin: 0;");
        sb.AppendLine("            padding: 0;");
        sb.AppendLine("            box-sizing: border-box;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        body {");
        sb.AppendLine("            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;");
        sb.AppendLine("            background-color: #f8f9fa;");
        sb.AppendLine("            color: #333;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .container {");
        sb.AppendLine("            display: flex;");
        sb.AppendLine("            height: 100vh;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .sidebar {");
        sb.AppendLine("            width: 280px;");
        sb.AppendLine("            background-color: #2c3e50;");
        sb.AppendLine("            color: white;");
        sb.AppendLine("            padding: 20px;");
        sb.AppendLine("            overflow-y: auto;");
        sb.AppendLine("            border-right: 3px solid #34495e;");
        sb.AppendLine("            min-width: 280px; // 防止侧边栏过度收缩");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .sidebar h1 {");
        sb.AppendLine("            font-size: 20px;");
        sb.AppendLine("            margin-bottom: 30px;");
        sb.AppendLine("            padding-bottom: 15px;");
        sb.AppendLine("            border-bottom: 2px solid #34495e;");
        sb.AppendLine("            color: #ecf0f1;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-group {");
        sb.AppendLine("            margin-bottom: 25px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-group h3 {");
        sb.AppendLine("            font-size: 14px;");
        sb.AppendLine("            color: #bdc3c7;");
        sb.AppendLine("            margin-bottom: 10px;");
        sb.AppendLine("            text-transform: uppercase;");
        sb.AppendLine("            letter-spacing: 1px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-item {");
        sb.AppendLine("            display: block;");
        sb.AppendLine("            padding: 12px 15px;");
        sb.AppendLine("            margin-bottom: 5px;");
        sb.AppendLine("            color: #ecf0f1;");
        sb.AppendLine("            text-decoration: none;");
        sb.AppendLine("            border-radius: 6px;");
        sb.AppendLine("            transition: all 0.3s ease;");
        sb.AppendLine("            cursor: pointer;");
        sb.AppendLine("            font-size: 14px;");
        sb.AppendLine("            white-space: nowrap;");
        sb.AppendLine("            overflow: hidden;");
        sb.AppendLine("            text-overflow: ellipsis;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-item:hover {");
        sb.AppendLine("            background-color: #34495e;");
        sb.AppendLine("            transform: translateX(5px);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-item.active {");
        sb.AppendLine("            background-color: #3498db;");
        sb.AppendLine("            color: white;");
        sb.AppendLine("            font-weight: 600;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-item.chain-item {");
        sb.AppendLine("            padding-left: 25px;");
        sb.AppendLine("            font-size: 13px;");
        sb.AppendLine("            color: #bdc3c7;");
        sb.AppendLine("            white-space: nowrap;");
        sb.AppendLine("            overflow: hidden;");
        sb.AppendLine("            text-overflow: ellipsis;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-item.chain-item:hover {");
        sb.AppendLine("            background-color: #34495e;");
        sb.AppendLine("            color: #ecf0f1;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-item.chain-item.active {");
        sb.AppendLine("            background-color: #e74c3c;");
        sb.AppendLine("            color: white;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .main-content {");
        sb.AppendLine("            flex: 1;");
        sb.AppendLine("            padding: 20px;");
        sb.AppendLine("            overflow-y: auto;");
        sb.AppendLine("            background-color: white;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .content-header {");
        sb.AppendLine("            margin-bottom: 20px;");
        sb.AppendLine("            padding-bottom: 15px;");
        sb.AppendLine("            border-bottom: 2px solid #ecf0f1;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .content-header h2 {");
        sb.AppendLine("            color: #2c3e50;");
        sb.AppendLine("            font-size: 24px;");
        sb.AppendLine("            margin-bottom: 5px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .content-header p {");
        sb.AppendLine("            color: #7f8c8d;");
        sb.AppendLine("            font-size: 14px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .diagram-container {");
        sb.AppendLine("            background-color: #fefefe;");
        sb.AppendLine("            border-radius: 8px;");
        sb.AppendLine("            padding: 20px;");
        sb.AppendLine("            box-shadow: 0 2px 10px rgba(0,0,0,0.1);");
        sb.AppendLine("            min-height: 600px;");
        sb.AppendLine("            height: auto;");
        sb.AppendLine("            text-align: center;");
        sb.AppendLine("            overflow: visible;");
        sb.AppendLine("            display: flex;");
        sb.AppendLine("            flex-direction: column;");
        sb.AppendLine("            justify-content: center;");
        sb.AppendLine("            align-items: center;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .mermaid {");
        sb.AppendLine("            display: block;");
        sb.AppendLine("            margin: 0 auto;");
        sb.AppendLine("            max-width: 100%;");
        sb.AppendLine("            width: 100%;");
        sb.AppendLine("            height: auto !important;");
        sb.AppendLine("            min-height: 400px;");
        sb.AppendLine("            overflow: visible;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .mermaid svg {");
        sb.AppendLine("            max-width: 100%;");
        sb.AppendLine("            height: auto !important;");
        sb.AppendLine("            min-height: 400px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .loading {");
        sb.AppendLine("            text-align: center;");
        sb.AppendLine("            padding: 60px;");
        sb.AppendLine("            color: #7f8c8d;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .loading::before {");
        sb.AppendLine("            content: '⏳';");
        sb.AppendLine("            font-size: 24px;");
        sb.AppendLine("            display: block;");
        sb.AppendLine("            margin-bottom: 10px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .error {");
        sb.AppendLine("            background-color: #ffebee;");
        sb.AppendLine("            color: #c62828;");
        sb.AppendLine("            padding: 15px;");
        sb.AppendLine("            border-radius: 6px;");
        sb.AppendLine("            border-left: 4px solid #f44336;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .chain-counter {");
        sb.AppendLine("            background-color: #3498db;");
        sb.AppendLine("            color: white;");
        sb.AppendLine("            border-radius: 50%;");
        sb.AppendLine("            width: 20px;");
        sb.AppendLine("            height: 20px;");
        sb.AppendLine("            display: inline-flex;");
        sb.AppendLine("            align-items: center;");
        sb.AppendLine("            justify-content: center;");
        sb.AppendLine("            font-size: 11px;");
        sb.AppendLine("            margin-left: 10px;");
        sb.AppendLine("            font-weight: bold;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .expand-toggle {");
        sb.AppendLine("            float: right;");
        sb.AppendLine("            font-size: 12px;");
        sb.AppendLine("            cursor: pointer;");
        sb.AppendLine("            color: #bdc3c7;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .expand-toggle:hover {");
        sb.AppendLine("            color: #ecf0f1;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .mermaid-live-button {");
        sb.AppendLine("            background: linear-gradient(135deg, #ff6b6b, #ee5a24);");
        sb.AppendLine("            color: white;");
        sb.AppendLine("            border: none;");
        sb.AppendLine("            padding: 8px 16px;");
        sb.AppendLine("            border-radius: 6px;");
        sb.AppendLine("            cursor: pointer;");
        sb.AppendLine("            font-size: 12px;");
        sb.AppendLine("            font-weight: 600;");
        sb.AppendLine("            text-decoration: none;");
        sb.AppendLine("            display: inline-flex;");
        sb.AppendLine("            align-items: center;");
        sb.AppendLine("            gap: 6px;");
        sb.AppendLine("            margin-top: 15px;");
        sb.AppendLine("            transition: all 0.3s ease;");
        sb.AppendLine("            box-shadow: 0 2px 4px rgba(238, 90, 36, 0.3);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .mermaid-live-button:hover {");
        sb.AppendLine("            background: linear-gradient(135deg, #ee5a24, #e55039);");
        sb.AppendLine("            transform: translateY(-2px);");
        sb.AppendLine("            box-shadow: 0 4px 8px rgba(238, 90, 36, 0.4);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .mermaid-live-button:active {");
        sb.AppendLine("            transform: translateY(0);");
        sb.AppendLine("            box-shadow: 0 2px 4px rgba(238, 90, 36, 0.3);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .diagram-header {");
        sb.AppendLine("            display: flex;");
        sb.AppendLine("            justify-content: space-between;");
        sb.AppendLine("            align-items: flex-start;");
        sb.AppendLine("            margin-bottom: 20px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .diagram-title-section {");
        sb.AppendLine("            flex: 1;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .chains-collapsed .chain-item {");
        sb.AppendLine("            display: none;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        /* 长文本处理 */");
        sb.AppendLine("        .nav-item {");
        sb.AppendLine("            max-width: 250px; // 限制最大宽度");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .nav-item.chain-item {");
        sb.AppendLine("            max-width: 230px;"); // 子菜单项稍微小一些
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        /* 搜索框样式 */");
        sb.AppendLine("        .search-container {");
        sb.AppendLine("            margin-bottom: 20px;");
        sb.AppendLine("            padding: 0 5px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-box {");
        sb.AppendLine("            width: 100%;");
        sb.AppendLine("            padding: 10px 12px;");
        sb.AppendLine("            border: 2px solid #34495e;");
        sb.AppendLine("            border-radius: 6px;");
        sb.AppendLine("            background-color: #34495e;");
        sb.AppendLine("            color: #ecf0f1;");
        sb.AppendLine("            font-size: 14px;");
        sb.AppendLine("            transition: all 0.3s ease;");
        sb.AppendLine("            outline: none;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-box::placeholder {");
        sb.AppendLine("            color: #bdc3c7;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-box:focus {");
        sb.AppendLine("            border-color: #3498db;");
        sb.AppendLine("            background-color: #2c3e50;");
        sb.AppendLine("            box-shadow: 0 0 0 2px rgba(52, 152, 219, 0.2);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-results {");
        sb.AppendLine("            margin-top: 10px;");
        sb.AppendLine();
        sb.AppendLine("        .search-result-item {");
        sb.AppendLine("            padding: 8px 12px;");
        sb.AppendLine("            margin-bottom: 3px;");
        sb.AppendLine("            background-color: #2c3e50;");
        sb.AppendLine("            border-radius: 4px;");
        sb.AppendLine("            cursor: pointer;");
        sb.AppendLine("            transition: all 0.2s ease;");
        sb.AppendLine("            font-size: 13px;");
        sb.AppendLine("            color: #ecf0f1;");
        sb.AppendLine("            border-left: 3px solid transparent;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-result-item:hover {");
        sb.AppendLine("            background-color: #34495e;");
        sb.AppendLine("            border-left-color: #3498db;");
        sb.AppendLine("            transform: translateX(3px);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-result-item.highlight {");
        sb.AppendLine("            background-color: #3498db;");
        sb.AppendLine("            border-left-color: #2980b9;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-no-results {");
        sb.AppendLine("            text-align: center;");
        sb.AppendLine("            color: #7f8c8d;");
        sb.AppendLine("            font-style: italic;");
        sb.AppendLine("            padding: 20px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        .search-category {");
        sb.AppendLine("            font-size: 11px;");
        sb.AppendLine("            color: #95a5a6;");
        sb.AppendLine("            margin-left: 8px;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        /* 响应式设计 */");
        sb.AppendLine("        @media (max-width: 768px) {");
        sb.AppendLine("            .container {");
        sb.AppendLine("                flex-direction: column;");
        sb.AppendLine("            }");
        sb.AppendLine("            ");
        sb.AppendLine("            .sidebar {");
        sb.AppendLine("                width: 100%;");
        sb.AppendLine("                height: auto;");
        sb.AppendLine("                max-height: 40vh;");
        sb.AppendLine("            }");
        sb.AppendLine("            ");
        sb.AppendLine("            .main-content {");
        sb.AppendLine("                flex: 1;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    </style>");
    }

    /// <summary>
    /// 添加HTML页面结构（含聚合关系图导航）
    /// </summary>
    private static void AddHtmlStructureWithAggregate(StringBuilder sb, int aggregateCount)
    {
        sb.AppendLine("    <div class=\"container\">");
        sb.AppendLine("        <div class=\"sidebar\">");
        sb.AppendLine("            <h1>🏗️ 架构图导航</h1>");
        sb.AppendLine("            ");
        sb.AppendLine("            <!-- 搜索框 -->");
        sb.AppendLine("            <div class=\"search-container\">");
        sb.AppendLine("                <input type=\"text\" id=\"searchBox\" class=\"search-box\" placeholder=\"搜索图表...\" oninput=\"performSearch()\">");
        sb.AppendLine("                <div id=\"searchResults\" class=\"search-results\" style=\"display: none;\"></div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            ");
        sb.AppendLine("            <div class=\"nav-group\">");
        sb.AppendLine("                <h3>图表展示</h3>");
        sb.AppendLine("                <a class=\"nav-item\" data-diagram=\"class\" href=\"#class\" title=\"🏛️ 架构大图\">");
        sb.AppendLine("                    🏛️ 架构大图");
        sb.AppendLine("                </a>");
        sb.AppendLine("                <a class=\"nav-item\" data-diagram=\"command\" href=\"#command\" title=\"⚡ 调用链路图\">");
        sb.AppendLine("                    ⚡ 命令关系图");
        sb.AppendLine("                </a>");
        sb.AppendLine("            </div>");
        sb.AppendLine();
        sb.AppendLine("            <div class=\"nav-group\">");
        sb.AppendLine("                <h3>聚合关系图 <span class=\"expand-toggle\" onclick=\"toggleAggregateDiagrams()\">▶</span> <span class=\"chain-counter\" id=\"aggregateDiagramCounter\">" + aggregateCount + "</span></h3>");
        sb.AppendLine("                <div class=\"chains-container\" id=\"aggregateDiagramsContainer\">");
        sb.AppendLine("                    <!-- 动态生成的聚合关系图菜单 -->");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine();
        sb.AppendLine("            <div class=\"nav-group\">");
        sb.AppendLine("                <h3>单独链路流程图 <span class=\"expand-toggle\" onclick=\"toggleIndividualChains()\">▶</span> <span class=\"chain-counter\" id=\"individualChainCounter\">0</span></h3>");
        sb.AppendLine("                <div class=\"chains-container\" id=\"individualChainsContainer\">");
        sb.AppendLine("                    <!-- 动态生成的单独链路流程图将在这里显示 -->");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine();
        sb.AppendLine("        <div class=\"main-content\">");
        sb.AppendLine("            <div class=\"content-header\">");
        sb.AppendLine("                <div class=\"diagram-header\">");
        sb.AppendLine("                    <div class=\"diagram-title-section\">");
        sb.AppendLine("                        <h2 id=\"diagramTitle\">选择图表类型</h2>");
        sb.AppendLine("                        <p id=\"diagramDescription\">请从左侧菜单选择要查看的图表类型</p>");
        sb.AppendLine("                    </div>");
        sb.AppendLine("                    <div class=\"diagram-actions\">");
        sb.AppendLine("                        <button id=\"mermaidLiveButton\" class=\"mermaid-live-button\" style=\"display: none;\" onclick=\"openInMermaidLive()\">");
        sb.AppendLine("                            🔗 View in Mermaid Live");
        sb.AppendLine("                        </button>");
        sb.AppendLine("                    </div>");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            ");
        sb.AppendLine("            <div class=\"diagram-container\">");
        sb.AppendLine("                <div id=\"diagramContent\">");
        sb.AppendLine("                    <div class=\"loading\">正在加载图表数据...</div>");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
    }

    /// <summary>
    /// 添加HTML JavaScript逻辑（含聚合关系图）
    /// </summary>
    private static void AddHtmlScriptWithAggregate(StringBuilder sb, CodeFlowAnalysisResult analysisResult,
        string commandFlowChart, string classDiagram, List<(string ChainName, string Diagram)> allChainFlowCharts,
        List<(string AggregateName, string Diagram)> allAggregateRelationDiagrams)
    {
        sb.AppendLine("    <script>");
        sb.AppendLine("        // 初始化 Mermaid");
        sb.AppendLine("        mermaid.initialize({");
        sb.AppendLine("            startOnLoad: false,");
        sb.AppendLine("            theme: 'base',");
        sb.AppendLine("            themeVariables: {");
        sb.AppendLine("                primaryColor: '#3498db',");
        sb.AppendLine("                primaryTextColor: '#2c3e50',");
        sb.AppendLine("                primaryBorderColor: '#2980b9',");
        sb.AppendLine("                lineColor: '#34495e',");
        sb.AppendLine("                secondaryColor: '#ecf0f1',");
        sb.AppendLine("                tertiaryColor: '#bdc3c7',");
        sb.AppendLine("                background: '#ffffff',");
        sb.AppendLine("                mainBkg: '#ffffff',");
        sb.AppendLine("                secondBkg: '#f8f9fa',");
        sb.AppendLine("                tertiaryBkg: '#ecf0f1'");
        sb.AppendLine("            },");
        sb.AppendLine("            flowchart: {");
        sb.AppendLine("                htmlLabels: true,");
        sb.AppendLine("                curve: 'basis',");
        sb.AppendLine("                diagramPadding: 20,");
        sb.AppendLine("                useMaxWidth: false,");
        sb.AppendLine("                useMaxHeight: false,");
        sb.AppendLine("                nodeSpacing: 50,");
        sb.AppendLine("                rankSpacing: 50");
        sb.AppendLine("            },");
        sb.AppendLine("            classDiagram: {");
        sb.AppendLine("                htmlLabels: true,");
        sb.AppendLine("                diagramPadding: 20,");
        sb.AppendLine("                useMaxWidth: false,");
        sb.AppendLine("                useMaxHeight: false");
        sb.AppendLine("            }");
        sb.AppendLine("        });");
        sb.AppendLine();

        // 添加分析结果数据
        AddAnalysisResultData(sb, analysisResult);

        // 添加图表数据
        AddDiagramDataWithAggregate(sb, commandFlowChart, classDiagram, allChainFlowCharts, allAggregateRelationDiagrams);

        // 添加JavaScript函数（含聚合关系图相关）
        AddJavaScriptFunctionsWithAggregate(sb);

        sb.AppendLine("    </script>");
    }

    /// <summary>
    /// 添加分析结果数据到JavaScript
    /// </summary>
    private static void AddAnalysisResultData(StringBuilder sb, CodeFlowAnalysisResult analysisResult)
    {
        sb.AppendLine("        // 分析结果数据");
        sb.AppendLine("        const analysisResult = {");

        // Controllers
        sb.AppendLine("            controllers: [");
        foreach (var controller in analysisResult.Controllers)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(controller.Name)}\", fullName: \"{EscapeJavaScript(controller.FullName)}\", methods: {FormatStringArray(controller.Methods)} }},");
        }
        sb.AppendLine("            ],");

        // Command Senders
        sb.AppendLine("            commandSenders: [");
        foreach (var sender in analysisResult.CommandSenders)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(sender.Name)}\", fullName: \"{EscapeJavaScript(sender.FullName)}\", methods: {FormatStringArray(sender.Methods)} }},");
        }
        sb.AppendLine("            ],");

        // Commands
        sb.AppendLine("            commands: [");
        foreach (var command in analysisResult.Commands)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(command.Name)}\", fullName: \"{EscapeJavaScript(command.FullName)}\" }},");
        }
        sb.AppendLine("            ],");

        // Entities
        sb.AppendLine("            entities: [");
        foreach (var entity in analysisResult.Entities)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(entity.Name)}\", fullName: \"{EscapeJavaScript(entity.FullName)}\", isAggregateRoot: {entity.IsAggregateRoot.ToString().ToLower()}, methods: {FormatStringArray(entity.Methods)} }},");
        }
        sb.AppendLine("            ],");

        // Domain Events
        sb.AppendLine("            domainEvents: [");
        foreach (var domainEvent in analysisResult.DomainEvents)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(domainEvent.Name)}\", fullName: \"{EscapeJavaScript(domainEvent.FullName)}\" }},");
        }
        sb.AppendLine("            ],");

        // Integration Events
        sb.AppendLine("            integrationEvents: [");
        foreach (var integrationEvent in analysisResult.IntegrationEvents)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(integrationEvent.Name)}\", fullName: \"{EscapeJavaScript(integrationEvent.FullName)}\" }},");
        }
        sb.AppendLine("            ],");

        // Domain Event Handlers
        sb.AppendLine("            domainEventHandlers: [");
        foreach (var handler in analysisResult.DomainEventHandlers)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(handler.Name)}\", fullName: \"{EscapeJavaScript(handler.FullName)}\", handledEventType: \"{EscapeJavaScript(handler.HandledEventType)}\", commands: {FormatStringArray(handler.Commands)} }},");
        }
        sb.AppendLine("            ],");

        // Integration Event Handlers
        sb.AppendLine("            integrationEventHandlers: [");
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(handler.Name)}\", fullName: \"{EscapeJavaScript(handler.FullName)}\", handledEventType: \"{EscapeJavaScript(handler.HandledEventType)}\", commands: {FormatStringArray(handler.Commands)} }},");
        }
        sb.AppendLine("            ],");

        // Integration Event Converters
        sb.AppendLine("            integrationEventConverters: [");
        foreach (var converter in analysisResult.IntegrationEventConverters)
        {
            sb.AppendLine($"                {{ name: \"{EscapeJavaScript(converter.Name)}\", fullName: \"{EscapeJavaScript(converter.FullName)}\", domainEventType: \"{EscapeJavaScript(converter.DomainEventType)}\", integrationEventType: \"{EscapeJavaScript(converter.IntegrationEventType)}\" }},");
        }
        sb.AppendLine("            ],");

        // Relationships
        sb.AppendLine("            relationships: [");
        foreach (var relationship in analysisResult.Relationships)
        {
            sb.AppendLine($"                {{ sourceType: \"{EscapeJavaScript(relationship.SourceType)}\", targetType: \"{EscapeJavaScript(relationship.TargetType)}\", callType: \"{EscapeJavaScript(relationship.CallType)}\", sourceMethod: \"{EscapeJavaScript(relationship.SourceMethod)}\", targetMethod: \"{EscapeJavaScript(relationship.TargetMethod)}\" }},");
        }
        sb.AppendLine("            ]");

        sb.AppendLine("        };");
        sb.AppendLine();
    }

    /// <summary>
    /// 添加图表数据到JavaScript（含聚合关系图）
    /// </summary>
    private static void AddDiagramDataWithAggregate(StringBuilder sb, string commandFlowChart, string classDiagram,
        List<(string ChainName, string Diagram)> allChainFlowCharts,
        List<(string AggregateName, string Diagram)> allAggregateRelationDiagrams)
    {
        sb.AppendLine("        // 图表配置");
        sb.AppendLine("        const diagramConfigs = {");
        sb.AppendLine("            class: {");
        sb.AppendLine("                title: '架构大图',");
        sb.AppendLine("                description: '展示系统中所有类型及其关系的完整视图'\n            },");
        sb.AppendLine("            command: {");
        sb.AppendLine("                title: '命令关系图',");
        sb.AppendLine("                description: '展示命令在系统中的完整流转与关系'\n            }");
        sb.AppendLine("        };");
        sb.AppendLine();

        sb.AppendLine("        // Mermaid图表数据");
        sb.AppendLine("        const diagrams = {");
        sb.AppendLine($"            class: `{EscapeJavaScriptTemplate(classDiagram)}`,");
        sb.AppendLine($"            command: `{EscapeJavaScriptTemplate(commandFlowChart)}`");
        sb.AppendLine("        };");
        sb.AppendLine();

        sb.AppendLine("        // 单独的链路流程图数据");
        sb.AppendLine("        const allChainFlowCharts = [");
        for (int i = 0; i < allChainFlowCharts.Count; i++)
        {
            var (chainName, diagram) = allChainFlowCharts[i];
            sb.AppendLine("            {");
            sb.AppendLine($"                name: \"{EscapeJavaScript(chainName)}\",");
            sb.AppendLine($"                diagram: `{EscapeJavaScriptTemplate(diagram)}`");
            sb.AppendLine($"            }}{(i < allChainFlowCharts.Count - 1 ? "," : "")}");
        }
        sb.AppendLine("        ];");
        sb.AppendLine();

        sb.AppendLine("        // 所有聚合关系图数据");
        sb.AppendLine("        const allAggregateRelationDiagrams = [");
        for (int i = 0; i < allAggregateRelationDiagrams.Count; i++)
        {
            var (aggName, diagram) = allAggregateRelationDiagrams[i];
            sb.AppendLine("            {");
            sb.AppendLine($"                name: \"{EscapeJavaScript(aggName)}\",");
            sb.AppendLine($"                diagram: `{EscapeJavaScriptTemplate(diagram)}`");
            sb.AppendLine($"            }}{(i < allAggregateRelationDiagrams.Count - 1 ? "," : "")}");
        }
        sb.AppendLine("        ];");
        sb.AppendLine();
    }

    /// <summary>
    /// 添加JavaScript函数（含聚合关系图相关）
    /// </summary>
    private static void AddJavaScriptFunctionsWithAggregate(StringBuilder sb)
    {
        sb.AppendLine("        let currentDiagram = null;");
        sb.AppendLine("        let currentDiagramData = null;");
        sb.AppendLine("        let individualChainsExpanded = false;");
        sb.AppendLine("        let aggregateDiagramsExpanded = false;");
        sb.AppendLine();
        sb.AppendLine("        // 初始化页面");
        sb.AppendLine("        function initializePage() {");
        sb.AppendLine("            generateAggregateDiagramNavigation();");
        sb.AppendLine("            generateChainNavigation();");
        sb.AppendLine("            addNavigationListeners();");
        sb.AppendLine("            addHashChangeListener();");
        sb.AppendLine("            handleInitialHash();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 生成聚合关系图导航");
        sb.AppendLine("        function generateAggregateDiagramNavigation() {");
        sb.AppendLine("            const container = document.getElementById('aggregateDiagramsContainer');");
        sb.AppendLine("            const counter = document.getElementById('aggregateDiagramCounter');");
        sb.AppendLine("            if (container && counter) {");
        sb.AppendLine("                container.innerHTML = '';");
        sb.AppendLine("                counter.textContent = allAggregateRelationDiagrams.length;");
        sb.AppendLine("                container.classList.add('chains-collapsed');");
        sb.AppendLine("                allAggregateRelationDiagrams.forEach((agg, index) => {");
        sb.AppendLine("                    const aggItem = document.createElement('a');");
        sb.AppendLine("                    aggItem.className = 'nav-item chain-item';");
        sb.AppendLine("                    aggItem.setAttribute('data-aggregate-diagram', index);");
        sb.AppendLine("                    const aggId = encodeURIComponent(agg.name.replace(/[^a-zA-Z0-9\u4e00-\u9fa5]/g, '-'));");
        sb.AppendLine("                    aggItem.href = `#aggregate-diagram-${aggId}`;");
        sb.AppendLine("                    aggItem.textContent = `🗂️ ${agg.name}`;");
        sb.AppendLine("                    aggItem.title = `🗂️ ${agg.name}`;");
        sb.AppendLine("                    container.appendChild(aggItem);");
        sb.AppendLine("                });");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 生成命令链路导航");
        sb.AppendLine("        function generateChainNavigation() {");
        sb.AppendLine("            // 单独链路流程图导航");
        sb.AppendLine("            const individualContainer = document.getElementById('individualChainsContainer');");
        sb.AppendLine("            const individualCounter = document.getElementById('individualChainCounter');");
        sb.AppendLine("            if (individualContainer && individualCounter) {");
        sb.AppendLine("                individualContainer.innerHTML = '';");
        sb.AppendLine("                individualCounter.textContent = allChainFlowCharts.length;");
        sb.AppendLine("                ");
        sb.AppendLine("                // 默认设置为折叠状态");
        sb.AppendLine("                individualContainer.classList.add('chains-collapsed');");
        sb.AppendLine("                allChainFlowCharts.forEach((chain, index) => {");
        sb.AppendLine("                    const chainItem = document.createElement('a');");
        sb.AppendLine("                    chainItem.className = 'nav-item chain-item';");
        sb.AppendLine("                    chainItem.setAttribute('data-individual-chain', index);");
        sb.AppendLine("                    const chainId = encodeURIComponent(chain.name.replace(/[^a-zA-Z0-9\\u4e00-\\u9fa5]/g, '-'));");
        sb.AppendLine("                    chainItem.href = `#individual-chain-${chainId}`;");
        sb.AppendLine("                    chainItem.textContent = `📊 ${chain.name}`;");
        sb.AppendLine("                    chainItem.title = `📊 ${chain.name}`;");
        sb.AppendLine("                    individualContainer.appendChild(chainItem);");
        sb.AppendLine("                });");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 添加导航事件监听");
        sb.AppendLine("        function addNavigationListeners() {");
        sb.AppendLine("            document.querySelectorAll('.nav-item[data-diagram]').forEach(item => {");
        sb.AppendLine("                item.addEventListener('click', (e) => {");
        sb.AppendLine("                    e.preventDefault();");
        sb.AppendLine("                    const diagramType = item.getAttribute('data-diagram');");
        sb.AppendLine("                    showDiagram(diagramType);");
        sb.AppendLine("                });");
        sb.AppendLine("            });");
        sb.AppendLine("            document.querySelectorAll('.nav-item[data-aggregate-diagram]').forEach(item => {");
        sb.AppendLine("                item.addEventListener('click', (e) => {");
        sb.AppendLine("                    e.preventDefault();");
        sb.AppendLine("                    const aggIndex = parseInt(item.getAttribute('data-aggregate-diagram'));");
        sb.AppendLine("                    if (!aggregateDiagramsExpanded) {");
        sb.AppendLine("                        toggleAggregateDiagrams();");
        sb.AppendLine("                    }");
        sb.AppendLine("                    showAggregateDiagram(aggIndex);");
        sb.AppendLine("                });");
        sb.AppendLine("            });");
        sb.AppendLine("            // ... 保持原有单独链路流程图监听 ...");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 显示图表");
        sb.AppendLine("        async function showDiagram(diagramType, updateHash = true) {");
        sb.AppendLine("            const config = diagramConfigs[diagramType];");
        sb.AppendLine("            if (!config) return;");
        sb.AppendLine();
        sb.AppendLine("            if (updateHash) {");
        sb.AppendLine("                window.location.hash = diagramType;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            document.querySelectorAll('.nav-item').forEach(item => {");
        sb.AppendLine("                item.classList.remove('active');");
        sb.AppendLine("            });");
        sb.AppendLine("            document.querySelector(`[data-diagram=\"${diagramType}\"]`).classList.add('active');");
        sb.AppendLine(); sb.AppendLine("            document.getElementById('diagramTitle').textContent = config.title;");
        sb.AppendLine("            document.getElementById('diagramDescription').textContent = config.description;");
        sb.AppendLine("            hideMermaidLiveButton();");
        sb.AppendLine();
        sb.AppendLine("            const contentDiv = document.getElementById('diagramContent');");
        sb.AppendLine("            contentDiv.innerHTML = '<div class=\"loading\">正在生成图表...</div>';");
        sb.AppendLine();
        sb.AppendLine("            try {");
        sb.AppendLine("                await new Promise(resolve => setTimeout(resolve, 300));");
        sb.AppendLine("                const diagramData = diagrams[diagramType];");
        sb.AppendLine("                if (!diagramData) {");
        sb.AppendLine("                    throw new Error('图表数据不存在');"); sb.AppendLine("                }");
        sb.AppendLine("                await renderMermaidDiagram(diagramData, contentDiv);");
        sb.AppendLine("                currentDiagram = diagramType;");
        sb.AppendLine("                currentDiagramData = diagramData;");
        sb.AppendLine("                showMermaidLiveButton();");        sb.AppendLine("            } catch (error) {");
        sb.AppendLine("                console.error('生成图表失败:', error);");
        sb.AppendLine("                const diagramData = diagrams[diagramType]; // 确保在错误处理中也能获取到数据");
        sb.AppendLine("                contentDiv.innerHTML = `<div class=\"error\">${formatErrorMessage('生成图表失败', error)}</div>`;");
        sb.AppendLine("                currentDiagram = diagramType;");
        sb.AppendLine("                currentDiagramData = diagramData || ''; // 如果数据不存在，使用空字符串");
        sb.AppendLine("                showMermaidLiveButton();");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine("        // 渲染Mermaid图表");
        sb.AppendLine("        async function renderMermaidDiagram(diagramData, container) {");
        sb.AppendLine("            const diagramId = `diagram-${Date.now()}`;");
        sb.AppendLine("            ");
        sb.AppendLine("            try {");
        sb.AppendLine("                container.innerHTML = '';");
        sb.AppendLine("                const diagramElement = document.createElement('div');");
        sb.AppendLine("                diagramElement.id = diagramId;");
        sb.AppendLine("                diagramElement.className = 'mermaid';");
        sb.AppendLine("                diagramElement.textContent = diagramData;");
        sb.AppendLine("                container.appendChild(diagramElement);");
        sb.AppendLine("                await mermaid.run({ nodes: [diagramElement] });");
        sb.AppendLine("            } catch (error) {");
        sb.AppendLine("                console.error('Mermaid渲染失败:', error);");
        sb.AppendLine("                // 确保在错误时也设置当前图表数据，这样按钮可以正常显示");
        sb.AppendLine("                currentDiagramData = diagramData;");
        sb.AppendLine("                throw error;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 显示单独链路流程图");
        sb.AppendLine("        async function showIndividualChain(chainIndex, updateHash = true) {");
        sb.AppendLine("            const chain = allChainFlowCharts[chainIndex];");
        sb.AppendLine("            if (!chain) return;");
        sb.AppendLine();
        sb.AppendLine("            // 如果单独链路流程图菜单是折叠的，则展开它");
        sb.AppendLine("            if (!individualChainsExpanded) {");
        sb.AppendLine("                toggleIndividualChains();");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (updateHash) {");
        sb.AppendLine("                const chainId = encodeURIComponent(chain.name.replace(/[^a-zA-Z0-9\\u4e00-\\u9fa5]/g, '-'));");
        sb.AppendLine("                window.location.hash = `individual-chain-${chainId}`;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            document.querySelectorAll('.nav-item').forEach(item => {");
        sb.AppendLine("                item.classList.remove('active');");
        sb.AppendLine("            });");
        sb.AppendLine("            document.querySelector(`[data-individual-chain=\"${chainIndex}\"]`).classList.add('active');");
        sb.AppendLine(); sb.AppendLine("            document.getElementById('diagramTitle').textContent = `${chain.name}`;");
        sb.AppendLine("            document.getElementById('diagramDescription').textContent = '单独链路的完整流程图';");
        sb.AppendLine("            hideMermaidLiveButton();");
        sb.AppendLine();
        sb.AppendLine("            const contentDiv = document.getElementById('diagramContent');");
        sb.AppendLine("            contentDiv.innerHTML = '<div class=\"loading\">正在生成单独链路图...</div>';");
        sb.AppendLine();
        sb.AppendLine("            try {");
        sb.AppendLine("                await new Promise(resolve => setTimeout(resolve, 200));");
        sb.AppendLine("                await renderMermaidDiagram(chain.diagram, contentDiv);");
        sb.AppendLine("                currentDiagram = `individual-chain-${chainIndex}`;");
        sb.AppendLine("                currentDiagramData = chain.diagram;");
        sb.AppendLine("                showMermaidLiveButton();");
        sb.AppendLine("            } catch (error) {");
        sb.AppendLine("                console.error('生成单独链路图失败:', error);");
        sb.AppendLine("                contentDiv.innerHTML = `<div class=\"error\">${formatErrorMessage('生成单独链路图失败', error)}</div>`;");
        sb.AppendLine("                currentDiagram = `individual-chain-${chainIndex}`;");
        sb.AppendLine("                currentDiagramData = chain.diagram;");
        sb.AppendLine("                showMermaidLiveButton();");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine("        // 格式化错误信息");
        sb.AppendLine("        function formatErrorMessage(prefix, error) {");
        sb.AppendLine("            let message = error.message;");
        sb.AppendLine("            if (message && message.toLowerCase().includes('too many edges')) {");
        sb.AppendLine("                return `${prefix}: ${message}。图表过于复杂，请点击 \"🔗 View in Mermaid Live\" 按钮在 Mermaid Live 中查看完整图表。`;");
        sb.AppendLine("            }");
        sb.AppendLine("            return `${prefix}: ${message}`;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 显示 Mermaid Live 按钮");
        sb.AppendLine("        function showMermaidLiveButton() {");
        sb.AppendLine("            const button = document.getElementById('mermaidLiveButton');");
        sb.AppendLine("            if (button) {");
        sb.AppendLine("                button.style.display = 'inline-flex';");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 隐藏 Mermaid Live 按钮");
        sb.AppendLine("        function hideMermaidLiveButton() {");
        sb.AppendLine("            const button = document.getElementById('mermaidLiveButton');");
        sb.AppendLine("            if (button) {");
        sb.AppendLine("                button.style.display = 'none';");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 在 Mermaid Live 中打开当前图表");
        sb.AppendLine("        function openInMermaidLive() {");
        sb.AppendLine("            if (!currentDiagramData) {");
        sb.AppendLine("                alert('没有可用的图表数据');");
        sb.AppendLine("                return;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            try {");
        sb.AppendLine("                // 检查是否有 pako 库可用");
        sb.AppendLine("                if (typeof pako !== 'undefined') {");
        sb.AppendLine("                    // 使用 pako 压缩方式 (首选)");
        sb.AppendLine("                    const state = {");
        sb.AppendLine("                        code: currentDiagramData,");
        sb.AppendLine("                        mermaid: {");
        sb.AppendLine("                            theme: 'default'");
        sb.AppendLine("                        },");
        sb.AppendLine("                        autoSync: true,");
        sb.AppendLine("                        updateDiagram: true");
        sb.AppendLine("                    };");
        sb.AppendLine();
        sb.AppendLine("                    const stateString = JSON.stringify(state);");
        sb.AppendLine("                    const compressed = pako.deflate(stateString, { to: 'string' });");
        sb.AppendLine("                    const encoded = btoa(String.fromCharCode.apply(null, compressed));");
        sb.AppendLine("                    const mermaidLiveUrl = `https://mermaid.live/edit#pako:${encoded}`;");
        sb.AppendLine();
        sb.AppendLine("                    window.open(mermaidLiveUrl, '_blank');");
        sb.AppendLine("                } else {");
        sb.AppendLine("                    // 回退到 base64 编码方式");
        sb.AppendLine("                    const encodedDiagram = btoa(unescape(encodeURIComponent(currentDiagramData)));");
        sb.AppendLine("                    const mermaidLiveUrl = `https://mermaid.live/edit#base64:${encodedDiagram}`;");
        sb.AppendLine("                    window.open(mermaidLiveUrl, '_blank');");
        sb.AppendLine("                }");
        sb.AppendLine("            } catch (error) {");
        sb.AppendLine("                console.error('无法打开 Mermaid Live:', error);");
        sb.AppendLine("                // 如果编码失败，尝试直接传递代码");
        sb.AppendLine("                try {");
        sb.AppendLine("                    const simpleEncodedDiagram = btoa(currentDiagramData);");
        sb.AppendLine("                    const fallbackUrl = `https://mermaid.live/edit#base64:${simpleEncodedDiagram}`;");
        sb.AppendLine("                    window.open(fallbackUrl, '_blank');");
        sb.AppendLine("                } catch (fallbackError) {");
        sb.AppendLine("                    console.error('备用方案也失败:', fallbackError);");
        sb.AppendLine("                    alert('无法打开 Mermaid Live，请检查浏览器控制台');");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 切换单独链路展开/收起");
        sb.AppendLine("        function toggleIndividualChains() {");
        sb.AppendLine("            individualChainsExpanded = !individualChainsExpanded;");
        sb.AppendLine("            const container = document.getElementById('individualChainsContainer');");
        sb.AppendLine("            const toggles = document.querySelectorAll('.expand-toggle');");
        sb.AppendLine("            const individualToggle = toggles[1]; // 第二个展开/收起按钮");
        sb.AppendLine("            ");
        sb.AppendLine("            if (individualChainsExpanded) {");
        sb.AppendLine("                container.classList.remove('chains-collapsed');");
        sb.AppendLine("                if (individualToggle) individualToggle.textContent = '▼';");
        sb.AppendLine("            } else {");
        sb.AppendLine("                container.classList.add('chains-collapsed');");
        sb.AppendLine("                if (individualToggle) individualToggle.textContent = '▶';");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 切换聚合关系图展开/收起");
        sb.AppendLine("        function toggleAggregateDiagrams() {");
        sb.AppendLine("            aggregateDiagramsExpanded = !aggregateDiagramsExpanded;");
        sb.AppendLine("            const container = document.getElementById('aggregateDiagramsContainer');");
        sb.AppendLine("            const toggles = document.querySelectorAll('.expand-toggle');");
        sb.AppendLine("            const aggToggle = toggles[0]; // 第一个展开/收起按钮");
        sb.AppendLine("            if (aggregateDiagramsExpanded) {");
        sb.AppendLine("                container.classList.remove('chains-collapsed');");
        sb.AppendLine("                if (aggToggle) aggToggle.textContent = '▼';");
        sb.AppendLine("            } else {");
        sb.AppendLine("                container.classList.add('chains-collapsed');");
        sb.AppendLine("                if (aggToggle) aggToggle.textContent = '▶';");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 添加URL哈希变化监听");
        sb.AppendLine("        function addHashChangeListener() {");
        sb.AppendLine("            window.addEventListener('hashchange', handleHashChange);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 根据聚合名称查找索引");
        sb.AppendLine("        function findAggregateIndexByName(aggName, aggArray) {");
        sb.AppendLine("            const decodedName = decodeURIComponent(aggName).replace(/-/g, ' ');");
        sb.AppendLine("            for (let i = 0; i < aggArray.length; i++) {");
        sb.AppendLine("                const normalizedAggName = aggArray[i].name.replace(/[^a-zA-Z0-9\u4e00-\u9fa5]/g, '-');");
        sb.AppendLine("                if (normalizedAggName === aggName || aggArray[i].name === decodedName) {");
        sb.AppendLine("                    return i;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("            return -1;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 处理URL哈希变化");
        sb.AppendLine("        function handleHashChange() {");
        sb.AppendLine("            const hash = window.location.hash.substring(1);");
        sb.AppendLine("            if (!hash) return;");
        sb.AppendLine("            if (diagramConfigs[hash]) {");
        sb.AppendLine("                showDiagram(hash, false);");
        sb.AppendLine("                return;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (hash.startsWith('aggregate-diagram-')) {");
        sb.AppendLine("                const aggName = hash.substring(18);");
        sb.AppendLine("                let aggIndex = findAggregateIndexByName(aggName, allAggregateRelationDiagrams);");
        sb.AppendLine("                if (aggIndex === -1) {");
        sb.AppendLine("                    aggIndex = parseInt(aggName);");
        sb.AppendLine("                }");
        sb.AppendLine("                if (!isNaN(aggIndex) && aggIndex >= 0 && aggIndex < allAggregateRelationDiagrams.length) {");
        sb.AppendLine("                    showAggregateDiagram(aggIndex, false);");
        sb.AppendLine("                    return;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("            // ... 保持原有单独链路流程图 hash 处理 ...");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 处理初始URL哈希");
        sb.AppendLine("        function handleInitialHash() {");
        sb.AppendLine("            const hash = window.location.hash.substring(1);");
        sb.AppendLine("            if (hash) {");
        sb.AppendLine("                handleHashChange();");
        sb.AppendLine("            } else {");
        sb.AppendLine("                showDiagram('class', false);");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 搜索功能");
        sb.AppendLine("        let allSearchableItems = [];");
        sb.AppendLine();
        sb.AppendLine("        // 初始化搜索数据");
        sb.AppendLine("        function initializeSearchData() {");
        sb.AppendLine("            allSearchableItems = [");
        sb.AppendLine("                { name: '架构大图', type: 'class', category: '图表展示', icon: '🏛️', target: 'class' },");
        sb.AppendLine("                { name: '命令关系图', type: 'command', category: '图表展示', icon: '⚡', target: 'command' }\n            ];");
        sb.AppendLine("            allAggregateRelationDiagrams.forEach((agg, index) => {");
        sb.AppendLine("                const aggId = encodeURIComponent(agg.name.replace(/[^a-zA-Z0-9\u4e00-\u9fa5]/g, '-'));");
        sb.AppendLine("                allSearchableItems.push({");
        sb.AppendLine("                    name: agg.name,");
        sb.AppendLine("                    type: 'aggregateDiagram',");
        sb.AppendLine("                    category: '聚合关系图',");
        sb.AppendLine("                    icon: '🗂️',");
        sb.AppendLine("                    target: `aggregate-diagram-${aggId}`,");
        sb.AppendLine("                    index: index");
        sb.AppendLine("                });");
        sb.AppendLine("            });");
        sb.AppendLine("            allChainFlowCharts.forEach((chain, index) => {");
        sb.AppendLine("                const chainId = encodeURIComponent(chain.name.replace(/[^a-zA-Z0-9\u4e00-\u9fa5]/g, '-'));");
        sb.AppendLine("                allSearchableItems.push({");
        sb.AppendLine("                    name: chain.name,");
        sb.AppendLine("                    type: 'individualChain',");
        sb.AppendLine("                    category: '单独链路流程图',");
        sb.AppendLine("                    icon: '📊',");
        sb.AppendLine("                    target: `individual-chain-${chainId}`,");
        sb.AppendLine("                    index: index");
        sb.AppendLine("                });");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        function selectSearchResult(item) {");
        sb.AppendLine("            document.getElementById('searchResults').style.display = 'none';");
        sb.AppendLine("            document.getElementById('searchBox').value = '';");
        sb.AppendLine("            switch (item.type) {");
        sb.AppendLine("                case 'command':");
        sb.AppendLine("                case 'class':");
        sb.AppendLine("                    window.location.hash = item.target;");
        sb.AppendLine("                    break;");
        sb.AppendLine("                case 'aggregateDiagram':");
        sb.AppendLine("                    if (!aggregateDiagramsExpanded) {");
        sb.AppendLine("                        toggleAggregateDiagrams();");
        sb.AppendLine("                    }");
        sb.AppendLine("                    window.location.hash = item.target;");
        sb.AppendLine("                    break;");
        sb.AppendLine("                case 'individualChain':");
        sb.AppendLine("                    if (!individualChainsExpanded) {");
        sb.AppendLine("                        toggleIndividualChains();");
        sb.AppendLine("                    }");
        sb.AppendLine("                    window.location.hash = item.target;");
        sb.AppendLine("                    break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // 点击页面其他地方隐藏搜索结果");
        sb.AppendLine("        document.addEventListener('click', function(event) {");
        sb.AppendLine("            const searchContainer = document.querySelector('.search-container');");
        sb.AppendLine("            if (!searchContainer.contains(event.target)) {");
        sb.AppendLine("                document.getElementById('searchResults').style.display = 'none';");
        sb.AppendLine("            }");
        sb.AppendLine("        });");
        sb.AppendLine();
        sb.AppendLine("        // 键盘导航支持");
        sb.AppendLine("        document.getElementById('searchBox').addEventListener('keydown', function(event) {");
        sb.AppendLine("            const searchResults = document.getElementById('searchResults');");
        sb.AppendLine("            const resultItems = searchResults.querySelectorAll('.search-result-item');");
        sb.AppendLine();
        sb.AppendLine("            if (event.key === 'Escape') {");
        sb.AppendLine("                searchResults.style.display = 'none';");
        sb.AppendLine("                this.value = '';");
        sb.AppendLine("                return;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (event.key === 'Enter' && resultItems.length > 0) {");
        sb.AppendLine("                const highlighted = searchResults.querySelector('.search-result-item.highlight');");
        sb.AppendLine("                if (highlighted) {");
        sb.AppendLine("                    highlighted.click();");
        sb.AppendLine("                } else {");
        sb.AppendLine("                    resultItems[0].click();");
        sb.AppendLine("                }");
        sb.AppendLine("                return;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {");
        sb.AppendLine("                event.preventDefault();");
        sb.AppendLine("                const highlighted = searchResults.querySelector('.search-result-item.highlight');");
        sb.AppendLine("                let nextIndex = 0;");
        sb.AppendLine();
        sb.AppendLine("                if (highlighted) {");
        sb.AppendLine("                    highlighted.classList.remove('highlight');");
        sb.AppendLine("                    const currentIndex = Array.from(resultItems).indexOf(highlighted);");
        sb.AppendLine("                    if (event.key === 'ArrowDown') {");
        sb.AppendLine("                        nextIndex = (currentIndex + 1) % resultItems.length;");
        sb.AppendLine("                    } else {");
        sb.AppendLine("                        nextIndex = (currentIndex - 1 + resultItems.length) % resultItems.length;");
        sb.AppendLine("                    }");
        sb.AppendLine("                }");
        sb.AppendLine();
        sb.AppendLine("                if (resultItems[nextIndex]) {");
        sb.AppendLine("                    resultItems[nextIndex].classList.add('highlight');");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        });");
        sb.AppendLine();
        sb.AppendLine("        // 页面加载完成后初始化");
        sb.AppendLine("        document.addEventListener('DOMContentLoaded', function() {");
        sb.AppendLine("            initializePage();");
        sb.AppendLine("            initializeSearchData();");
        sb.AppendLine("        });");
        // 兼容全局调用（如HTML onclick）
        sb.AppendLine("        window.showAggregateDiagram = showAggregateDiagram;");
        sb.AppendLine("        // 显示聚合关系图");
        sb.AppendLine("        async function showAggregateDiagram(aggIndex, updateHash = true) {");
        sb.AppendLine("            const agg = allAggregateRelationDiagrams[aggIndex];");
        sb.AppendLine("            if (!agg) return;");
        sb.AppendLine();
        // 如果聚合关系图菜单是折叠的，则展开它
        sb.AppendLine("            if (!aggregateDiagramsExpanded) {");
        sb.AppendLine("                toggleAggregateDiagrams();");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (updateHash) {");
        sb.AppendLine("                const aggId = encodeURIComponent(agg.name.replace(/[^a-zA-Z0-9\u4e00-\u9fa5]/g, '-'));");
        sb.AppendLine("                window.location.hash = `aggregate-diagram-${aggId}`;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            document.querySelectorAll('.nav-item').forEach(item => {");
        sb.AppendLine("                item.classList.remove('active');");
        sb.AppendLine("            });");
        sb.AppendLine("            document.querySelector(`[data-aggregate-diagram=\"${aggIndex}\"]`).classList.add('active');");
        sb.AppendLine();
        sb.AppendLine("            document.getElementById('diagramTitle').textContent = `${agg.name}`;");
        sb.AppendLine("            document.getElementById('diagramDescription').textContent = '聚合根相关的关系图';");
        sb.AppendLine("            hideMermaidLiveButton();");
        sb.AppendLine();
        sb.AppendLine("            const contentDiv = document.getElementById('diagramContent');");
        sb.AppendLine("            contentDiv.innerHTML = '<div class=\"loading\">正在生成聚合关系图...</div>';");
        sb.AppendLine();
        sb.AppendLine("            try {");
        sb.AppendLine("                await new Promise(resolve => setTimeout(resolve, 200));");
        sb.AppendLine("                await renderMermaidDiagram(agg.diagram, contentDiv);");
        sb.AppendLine("                currentDiagram = `aggregate-diagram-${aggIndex}`;");
        sb.AppendLine("                currentDiagramData = agg.diagram;");
        sb.AppendLine("                showMermaidLiveButton();");
        sb.AppendLine("            } catch (error) {");
        sb.AppendLine("                console.error('生成聚合关系图失败:', error);");
        sb.AppendLine("                contentDiv.innerHTML = `<div class=\"error\">${formatErrorMessage('生成聚合关系图失败', error)}</div>`;");
        sb.AppendLine("                currentDiagram = `aggregate-diagram-${aggIndex}`;");
        sb.AppendLine("                currentDiagramData = agg.diagram;");
        sb.AppendLine("                showMermaidLiveButton();");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    /// <summary>
    /// HTML转义
    /// </summary>
    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("\"", "&quot;")
                   .Replace("'", "&#39;");
    }

    /// <summary>
    /// JavaScript字符串转义
    /// </summary>
    private static string EscapeJavaScript(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("'", "\\'")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\t", "\\t")
                   .Replace("<", "\\u003c")
                   .Replace(">", "\\u003e");
    }

    /// <summary>
    /// JavaScript模板字符串转义
    /// </summary>
    private static string EscapeJavaScriptTemplate(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace("\\", "\\\\")
                   .Replace("`", "\\`")
                   .Replace("${", "\\${");
    }

    /// <summary>
    /// 格式化字符串数组为JavaScript数组
    /// </summary>
    private static string FormatStringArray(IEnumerable<string> strings)
    {
        if (strings == null) return "[]";
        var escaped = strings.Select(s => $"\"{EscapeJavaScript(s)}\"");
        return $"[{string.Join(", ", escaped)}]";
    }

    /// <summary>
    /// 获取节点样式类
    /// </summary>
    private static string GetNodeStyleClass(string nodeFullName, CodeFlowAnalysisResult analysisResult)
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

    /// <summary>
    /// 生成所有聚合关系图的集合
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>包含所有聚合关系图的元组列表，每个聚合根对应一张图</returns>
    public static List<(string AggregateName, string Diagram)> GenerateAllAggregateRelationDiagrams(CodeFlowAnalysisResult analysisResult)
    {
        var result = new List<(string AggregateName, string Diagram)>();
        foreach (var entity in analysisResult.Entities.Where(e => e.IsAggregateRoot))
        {
            var diagram = GenerateAggregateRelationDiagram(analysisResult, entity.FullName);
            result.Add((entity.Name, diagram));
        }
        return result;
    }

    #endregion
}
