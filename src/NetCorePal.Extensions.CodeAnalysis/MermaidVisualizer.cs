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
        foreach (var sender in analysisResult.CommandSenders.Where(s =>
                     involvedTypes.Contains(s.FullName) && !controllerFullNames.Contains(s.FullName)))
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
            var shape = entity.IsAggregateRoot
                ? "{{" + EscapeMermaidText(entity.Name) + "}}"
                : "[" + EscapeMermaidText(entity.Name) + "]";
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
                var label = GetRelationshipLabel(relationship.CallType, relationship.SourceMethod,
                    relationship.TargetMethod);
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
    public static string GenerateAggregateRelationDiagram(CodeFlowAnalysisResult analysisResult,
        string aggregateFullName)
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

                    var integrationEvent =
                        analysisResult.IntegrationEvents.FirstOrDefault(i => i.FullName == currentType);
                    if (integrationEvent != null)
                    {
                        sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(integrationEvent.Name)}\"]");
                        sb.AppendLine($"    class {nodeId} integrationEvent;");
                    }

                    var domainEventHandler =
                        analysisResult.DomainEventHandlers.FirstOrDefault(h => h.FullName == currentType);
                    if (domainEventHandler != null)
                    {
                        sb.AppendLine(
                            $"    {nodeId}[\"{EscapeMermaidText(GetClassNameFromFullName(domainEventHandler.FullName))}\"]");
                        sb.AppendLine($"    class {nodeId} handler;");
                    }

                    var integrationEventHandler =
                        analysisResult.IntegrationEventHandlers.FirstOrDefault(h => h.FullName == currentType);
                    if (integrationEventHandler != null)
                    {
                        sb.AppendLine(
                            $"    {nodeId}[\"{EscapeMermaidText(GetClassNameFromFullName(integrationEventHandler.FullName))}\"]");
                        sb.AppendLine($"    class {nodeId} handler;");
                    }
                }

                processedNodes.Add(currentType);
            }

            // 递归下游
            foreach (var rel in analysisResult.Relationships.Where(r => r.SourceType == currentType))
            {
                var targetEntity = analysisResult.Entities.FirstOrDefault(e => e.FullName == rel.TargetType);
                bool targetIsOtherAggregate = targetEntity != null && targetEntity.IsAggregateRoot &&
                                              rel.TargetType != aggregateFullName;
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
                bool sourceIsOtherAggregate = sourceEntity != null && sourceEntity.IsAggregateRoot &&
                                              rel.SourceType != aggregateFullName;
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
    public static List<(string ChainName, string Diagram)> GenerateAllChainFlowCharts(
        CodeFlowAnalysisResult analysisResult)
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
    private static
        List<(string ChainName, List<string> ChainNodes, List<(string Source, string Target, string Label)>
            ChainRelations,
            Dictionary<string, string> ChainNodeIds)> GenerateMultiChainGroups(CodeFlowAnalysisResult analysisResult)
    {
        var globalNodeCounter = 1;
        var chainGroups =
            new List<(string ChainName, List<string> ChainNodes, List<(string Source, string Target, string Label)>
                ChainRelations, Dictionary<string, string> ChainNodeIds)>();

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
                    BuildChainFromCommand(analysisResult, relation.TargetType, startNode, chainNodes, chainRelations,
                        localProcessedNodes);
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
                    BuildChainFromCommand(analysisResult, commandType, startNode, chainNodes, chainRelations,
                        localProcessedNodes);
                }
            }
        }
    }

    /// <summary>
    /// 从命令开始构建链路
    /// </summary>
    private static void BuildChainFromCommand(CodeFlowAnalysisResult analysisResult, string commandType,
        string sourceNode,
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
            .Where(r => r.SourceType == aggregateType && r.SourceMethod == aggregateMethod &&
                        r.CallType == "MethodToDomainEvent")
            .ToList();

        foreach (var eventRelation in domainEventRelations)
        {
            var eventType = eventRelation.TargetType;

            if (!localProcessedNodes.Contains(eventType))
            {
                BuildChainFromDomainEvent(analysisResult, eventType, aggregateMethodNode, chainNodes, chainRelations,
                    localProcessedNodes);
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
    private static void BuildChainFromDomainEvent(CodeFlowAnalysisResult analysisResult, string eventType,
        string sourceNode,
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
                        BuildChainFromCommand(analysisResult, commandType, handler.FullName, chainNodes, chainRelations,
                            localProcessedNodes);
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
                    BuildChainFromIntegrationEvent(analysisResult, integrationEventType, converter.FullName, chainNodes,
                        chainRelations, localProcessedNodes);
                }
            }
        }
    }

    /// <summary>
    /// 从集成事件构建链路
    /// </summary>
    private static void BuildChainFromIntegrationEvent(CodeFlowAnalysisResult analysisResult,
        string integrationEventType, string sourceNode,
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
                        BuildChainFromCommand(analysisResult, commandType, integrationHandler.FullName, chainNodes,
                            chainRelations, localProcessedNodes);
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
                                    TraceChainExecution(analysisResult, commandType, chainNodes, chainRelations,
                                        visitedInChain);
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
        var integrationEventHandler =
            analysisResult.IntegrationEventHandlers.FirstOrDefault(h => h.FullName == nodeType);

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
            var shape = entity.IsAggregateRoot
                ? "{{" + EscapeMermaidText(displayName) + "}}"
                : "[" + EscapeMermaidText(displayName) + "]";
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
    public static string GenerateVisualizationHtml(CodeFlowAnalysisResult analysisResult,
        string title = "NetCorePal 架构图可视化")
    {
        return string.Empty;
    }


    /// <summary>
    /// 生成所有聚合关系图的集合
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>包含所有聚合关系图的元组列表，每个聚合根对应一张图</returns>
    public static List<(string AggregateName, string Diagram)> GenerateAllAggregateRelationDiagrams(
        CodeFlowAnalysisResult analysisResult)
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