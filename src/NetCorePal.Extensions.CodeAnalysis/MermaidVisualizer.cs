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
                var label = GetRelationshipLabel(relationship.CallType, relationship.SourceMethod, relationship.TargetMethod);
                
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

    /// <summary>
    /// 生成命令链路流程图（以发出命令的地方为起点，分别展示一条条链路）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>包含每个命令链路的 Mermaid 流程图字符串列表</returns>
    public static List<(string ChainName, string MermaidDiagram)> GenerateCommandChainFlowCharts(CodeFlowAnalysisResult analysisResult)
    {
        var chains = new List<(string ChainName, string MermaidDiagram)>();
        var processedChains = new HashSet<string>();

        // 找出所有发出命令的起点（通常是控制器或事件处理器）
        var commandSenders = analysisResult.Relationships
            .Where(r => r.CallType == "MethodToCommand")
            .GroupBy(r => r.SourceType)
            .ToList();

        foreach (var senderGroup in commandSenders)
        {
            var senderType = senderGroup.Key;
            var senderName = GetClassNameFromFullName(senderType);
            
            // 为每个发送者的每个命令创建一个链路图
            foreach (var commandRelation in senderGroup)
            {
                var chainKey = $"{senderType}-{commandRelation.TargetType}";
                if (processedChains.Contains(chainKey))
                    continue;

                processedChains.Add(chainKey);

                var commandType = commandRelation.TargetType;
                var commandName = GetClassNameFromFullName(commandType);
                var chainName = $"{senderName} -> {commandName}";

                var diagram = GenerateSingleCommandChain(analysisResult, senderType, commandType, commandRelation.SourceMethod);
                chains.Add((chainName, diagram));
            }
        }

        // 也为集成事件处理器发出的命令创建链路图
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            foreach (var commandType in handler.Commands)
            {
                var chainKey = $"{handler.FullName}-{commandType}";
                if (processedChains.Contains(chainKey))
                    continue;

                processedChains.Add(chainKey);

                var commandName = GetClassNameFromFullName(commandType);
                var chainName = $"{handler.Name} -> {commandName}";

                var diagram = GenerateSingleCommandChain(analysisResult, handler.FullName, commandType, "Handle");
                chains.Add((chainName, diagram));
            }
        }

        return chains;
    }

    /// <summary>
    /// 生成单个命令链路的流程图
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <param name="startType">起点类型</param>
    /// <param name="commandType">命令类型</param>
    /// <param name="startMethod">起点方法</param>
    /// <returns>Mermaid 流程图字符串</returns>
    private static string GenerateSingleCommandChain(CodeFlowAnalysisResult analysisResult, string startType, string commandType, string startMethod)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");
        sb.AppendLine();

        var nodeIds = new Dictionary<string, string>();
        var nodeIdCounter = 1;
        var visitedNodes = new HashSet<string>();

        string GetNodeId(string fullName, string nodeType)
        {
            var key = $"{nodeType}_{fullName}";
            if (!nodeIds.ContainsKey(key))
            {
                nodeIds[key] = $"{nodeType}{nodeIdCounter++}";
            }
            return nodeIds[key];
        }

        // 添加起点节点
        AddChainNode(sb, startType, GetNodeId(startType, "START"), analysisResult, visitedNodes);

        // 添加命令节点
        AddChainNode(sb, commandType, GetNodeId(commandType, "CMD"), analysisResult, visitedNodes);

        // 跟踪命令执行链路
        TraceCommandExecution(sb, analysisResult, commandType, nodeIds, visitedNodes);

        sb.AppendLine();
        sb.AppendLine("    %% Chain Relationships");

        // 添加起点到命令的关系
        var startNodeId = GetNodeId(startType, "START");
        var commandNodeId = GetNodeId(commandType, "CMD");
        sb.AppendLine($"    {startNodeId} -->|{EscapeMermaidText(startMethod)}| {commandNodeId}");

        // 添加命令执行链路中的关系
        AddChainRelationships(sb, analysisResult, commandType, nodeIds, visitedNodes);

        sb.AppendLine();
        AddChainStyles(sb);

        return sb.ToString();
    }

    /// <summary>
    /// 添加链路中的节点
    /// </summary>
    private static void AddChainNode(StringBuilder sb, string nodeType, string nodeId, CodeFlowAnalysisResult analysisResult, HashSet<string> visitedNodes)
    {
        if (visitedNodes.Contains(nodeType))
            return;

        visitedNodes.Add(nodeType);
        var nodeName = GetClassNameFromFullName(nodeType);

        // 根据节点类型确定样式
        var controller = analysisResult.Controllers.FirstOrDefault(c => c.FullName == nodeType);
        var command = analysisResult.Commands.FirstOrDefault(c => c.FullName == nodeType);
        var entity = analysisResult.Entities.FirstOrDefault(e => e.FullName == nodeType);
        var domainEvent = analysisResult.DomainEvents.FirstOrDefault(d => d.FullName == nodeType);
        var integrationEvent = analysisResult.IntegrationEvents.FirstOrDefault(i => i.FullName == nodeType);
        var domainEventHandler = analysisResult.DomainEventHandlers.FirstOrDefault(h => h.FullName == nodeType);
        var integrationEventHandler = analysisResult.IntegrationEventHandlers.FirstOrDefault(h => h.FullName == nodeType);

        if (controller != null)
        {
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else if (command != null)
        {
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else if (entity != null)
        {
            var shape = entity.IsAggregateRoot ? "{{" + EscapeMermaidText(nodeName) + "}}" : "[" + EscapeMermaidText(nodeName) + "]";
            sb.AppendLine($"    {nodeId}{shape}");
        }
        else if (domainEvent != null)
        {
            sb.AppendLine($"    {nodeId}(\"{EscapeMermaidText(nodeName)}\")");
        }
        else if (integrationEvent != null)
        {
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else if (domainEventHandler != null)
        {
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else if (integrationEventHandler != null)
        {
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else
        {
            sb.AppendLine($"    {nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
    }

    /// <summary>
    /// 跟踪命令执行链路
    /// </summary>
    private static void TraceCommandExecution(StringBuilder sb, CodeFlowAnalysisResult analysisResult, string commandType, Dictionary<string, string> nodeIds, HashSet<string> visitedNodes)
    {
        var commandRelations = analysisResult.Relationships
            .Where(r => r.SourceType == commandType)
            .ToList();

        foreach (var relation in commandRelations)
        {
            var targetType = relation.TargetType;
            var targetNodeId = GetOrCreateNodeId(targetType, nodeIds);

            // 添加目标节点
            AddChainNode(sb, targetType, targetNodeId, analysisResult, visitedNodes);

            // 如果目标是聚合根，继续跟踪它产生的领域事件
            var targetEntity = analysisResult.Entities.FirstOrDefault(e => e.FullName == targetType);
            if (targetEntity != null && targetEntity.IsAggregateRoot)
            {
                TraceDomainEventsFromAggregate(sb, analysisResult, targetType, nodeIds, visitedNodes);
            }
        }
    }

    /// <summary>
    /// 跟踪聚合根产生的领域事件
    /// </summary>
    private static void TraceDomainEventsFromAggregate(StringBuilder sb, CodeFlowAnalysisResult analysisResult, string aggregateType, Dictionary<string, string> nodeIds, HashSet<string> visitedNodes)
    {
        // 查找从聚合根方法发出的领域事件
        var domainEventRelations = analysisResult.Relationships
            .Where(r => r.SourceType == aggregateType && r.CallType == "DomainEventToHandler")
            .ToList();

        foreach (var relation in domainEventRelations)
        {
            var eventType = relation.TargetType;
            var eventNodeId = GetOrCreateNodeId(eventType, nodeIds);

            // 添加领域事件节点
            AddChainNode(sb, eventType, eventNodeId, analysisResult, visitedNodes);

            // 跟踪事件处理器
            TraceEventHandlers(sb, analysisResult, eventType, nodeIds, visitedNodes);
        }
    }

    /// <summary>
    /// 跟踪事件处理器
    /// </summary>
    private static void TraceEventHandlers(StringBuilder sb, CodeFlowAnalysisResult analysisResult, string eventType, Dictionary<string, string> nodeIds, HashSet<string> visitedNodes)
    {
        // 查找处理该事件的处理器
        var handlers = analysisResult.DomainEventHandlers
            .Where(h => h.HandledEventType == eventType)
            .ToList();

        foreach (var handler in handlers)
        {
            var handlerNodeId = GetOrCreateNodeId(handler.FullName, nodeIds);

            // 添加处理器节点
            AddChainNode(sb, handler.FullName, handlerNodeId, analysisResult, visitedNodes);

            // 跟踪处理器发出的命令
            foreach (var commandType in handler.Commands)
            {
                var commandNodeId = GetOrCreateNodeId(commandType, nodeIds);
                AddChainNode(sb, commandType, commandNodeId, analysisResult, visitedNodes);

                // 递归跟踪命令执行
                TraceCommandExecution(sb, analysisResult, commandType, nodeIds, visitedNodes);
            }
        }

        // 查找集成事件转换器
        var converters = analysisResult.IntegrationEventConverters
            .Where(c => c.DomainEventType == eventType)
            .ToList();

        foreach (var converter in converters)
        {
            var converterNodeId = GetOrCreateNodeId(converter.FullName, nodeIds);
            var integrationEventNodeId = GetOrCreateNodeId(converter.IntegrationEventType, nodeIds);

            // 添加转换器和集成事件节点
            AddChainNode(sb, converter.FullName, converterNodeId, analysisResult, visitedNodes);
            AddChainNode(sb, converter.IntegrationEventType, integrationEventNodeId, analysisResult, visitedNodes);

            // 跟踪集成事件处理器
            var integrationHandlers = analysisResult.IntegrationEventHandlers
                .Where(h => h.HandledEventType == converter.IntegrationEventType)
                .ToList();

            foreach (var integrationHandler in integrationHandlers)
            {
                var integrationHandlerNodeId = GetOrCreateNodeId(integrationHandler.FullName, nodeIds);
                AddChainNode(sb, integrationHandler.FullName, integrationHandlerNodeId, analysisResult, visitedNodes);

                // 跟踪集成事件处理器发出的命令
                foreach (var commandType in integrationHandler.Commands)
                {
                    var commandNodeId = GetOrCreateNodeId(commandType, nodeIds);
                    AddChainNode(sb, commandType, commandNodeId, analysisResult, visitedNodes);

                    // 递归跟踪命令执行
                    TraceCommandExecution(sb, analysisResult, commandType, nodeIds, visitedNodes);
                }
            }
        }
    }

    /// <summary>
    /// 添加链路中的关系
    /// </summary>
    private static void AddChainRelationships(StringBuilder sb, CodeFlowAnalysisResult analysisResult, string commandType, Dictionary<string, string> nodeIds, HashSet<string> visitedNodes)
    {
        var processedRelations = new HashSet<string>();

        void AddRelationship(string sourceType, string targetType, string callType, string sourceMethod = "", string targetMethod = "")
        {
            var relationKey = $"{sourceType}-{targetType}-{callType}";
            if (processedRelations.Contains(relationKey))
                return;

            processedRelations.Add(relationKey);

            var sourceNodeId = FindNodeId(nodeIds, sourceType);
            var targetNodeId = FindNodeId(nodeIds, targetType);

            if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
            {
                var arrow = GetArrowStyle(callType);
                var label = GetRelationshipLabel(callType, sourceMethod, targetMethod);

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

        // 添加命令相关的关系
        var commandRelations = analysisResult.Relationships
            .Where(r => visitedNodes.Contains(r.SourceType) && visitedNodes.Contains(r.TargetType))
            .ToList();

        foreach (var relation in commandRelations)
        {
            AddRelationship(relation.SourceType, relation.TargetType, relation.CallType, relation.SourceMethod, relation.TargetMethod);
        }

        // 添加领域事件处理器到命令的关系
        foreach (var handler in analysisResult.DomainEventHandlers)
        {
            if (!visitedNodes.Contains(handler.FullName))
                continue;

            foreach (var commandTypeInHandler in handler.Commands)
            {
                if (visitedNodes.Contains(commandTypeInHandler))
                {
                    AddRelationship(handler.FullName, commandTypeInHandler, "HandlerToCommand");
                }
            }
        }

        // 添加集成事件处理器到命令的关系
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            if (!visitedNodes.Contains(handler.FullName))
                continue;

            foreach (var commandTypeInHandler in handler.Commands)
            {
                if (visitedNodes.Contains(commandTypeInHandler))
                {
                    AddRelationship(handler.FullName, commandTypeInHandler, "HandlerToCommand");
                }
            }
        }

        // 添加转换器关系
        foreach (var converter in analysisResult.IntegrationEventConverters)
        {
            if (visitedNodes.Contains(converter.DomainEventType) && visitedNodes.Contains(converter.IntegrationEventType))
            {
                AddRelationship(converter.DomainEventType, converter.IntegrationEventType, "DomainEventToIntegrationEvent");
            }
        }
    }

    /// <summary>
    /// 获取或创建节点ID
    /// </summary>
    private static string GetOrCreateNodeId(string fullName, Dictionary<string, string> nodeIds)
    {
        var existingId = FindNodeId(nodeIds, fullName);
        if (!string.IsNullOrEmpty(existingId))
            return existingId;

        var nodeType = GetNodeTypeFromFullName(fullName);
        var key = $"{nodeType}_{fullName}";
        var nodeId = $"{nodeType}{nodeIds.Count + 1}";
        nodeIds[key] = nodeId;
        return nodeId;
    }

    /// <summary>
    /// 从完整名称推断节点类型
    /// </summary>
    private static string GetNodeTypeFromFullName(string fullName)
    {
        var className = GetClassNameFromFullName(fullName);
        
        if (className.EndsWith("Controller"))
            return "C";
        if (className.EndsWith("Command"))
            return "CMD";
        if (className.EndsWith("Event"))
            return className.Contains("Integration") ? "IE" : "DE";
        if (className.EndsWith("Handler"))
            return className.Contains("Integration") ? "IEH" : "DEH";
        if (className.EndsWith("Converter"))
            return "IEC";
        
        return "N"; // 默认节点类型
    }

    /// <summary>
    /// 添加链路图样式
    /// </summary>
    private static void AddChainStyles(StringBuilder sb)
    {
        sb.AppendLine("    %% Chain Styles");
        sb.AppendLine("    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:2px;");
        sb.AppendLine("    classDef command fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;");
        sb.AppendLine("    classDef entity fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px;");
        sb.AppendLine("    classDef domainEvent fill:#fff3e0,stroke:#e65100,stroke-width:2px;");
        sb.AppendLine("    classDef integrationEvent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef handler fill:#f1f8e9,stroke:#33691e,stroke-width:2px;");
        sb.AppendLine("    classDef converter fill:#e3f2fd,stroke:#0277bd,stroke-width:2px;");
        sb.AppendLine();
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

    /// <summary>
    /// 生成多链路流程图（在一张图中展示多个命令链路，按链路分组显示）
    /// </summary>
    /// <param name="analysisResult">代码分析结果</param>
    /// <returns>包含多个链路的 Mermaid 流程图字符串</returns>
    public static string GenerateMultiChainFlowChart(CodeFlowAnalysisResult analysisResult)
    {
        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");
        sb.AppendLine();

        var globalNodeCounter = 1;
        var chainGroups = new List<(string ChainName, List<string> ChainNodes, List<(string Source, string Target, string Label)> ChainRelations, Dictionary<string, string> ChainNodeIds)>();

        // 找出所有发出命令的起点
        var commandSenders = analysisResult.Relationships
            .Where(r => r.CallType == "MethodToCommand")
            .GroupBy(r => r.SourceType)
            .ToList();

        var processedChains = new HashSet<string>();
        int chainIndex = 1;

        // 为每个命令链路收集节点和关系
        foreach (var senderGroup in commandSenders)
        {
            var senderType = senderGroup.Key;
            
            foreach (var commandRelation in senderGroup)
            {
                var chainKey = $"{senderType}-{commandRelation.TargetType}";
                if (processedChains.Contains(chainKey))
                    continue;

                processedChains.Add(chainKey);

                var commandType = commandRelation.TargetType;
                var senderName = GetClassNameFromFullName(senderType);
                var commandName = GetClassNameFromFullName(commandType);
                var chainName = $"Chain{chainIndex}: {senderName} → {commandName}";

                // 为每个链路创建独立的节点ID映射
                var chainNodeIds = new Dictionary<string, string>();
                var chainNodes = new List<string>();
                var chainRelations = new List<(string Source, string Target, string Label)>();
                var visitedInChain = new HashSet<string>();

                // 收集这个链路的所有节点和关系
                CollectChainData(analysisResult, senderType, commandType, commandRelation.SourceMethod, 
                    chainNodes, chainRelations, visitedInChain, chainNodeIds, ref globalNodeCounter);

                chainGroups.Add((chainName, chainNodes, chainRelations, chainNodeIds));
                chainIndex++;
            }
        }

        // 处理集成事件处理器发出的命令链路
        foreach (var handler in analysisResult.IntegrationEventHandlers)
        {
            foreach (var commandType in handler.Commands)
            {
                var chainKey = $"{handler.FullName}-{commandType}";
                if (processedChains.Contains(chainKey))
                    continue;

                processedChains.Add(chainKey);

                var commandName = GetClassNameFromFullName(commandType);
                var chainName = $"Chain{chainIndex}: {handler.Name} → {commandName}";

                // 为每个链路创建独立的节点ID映射
                var chainNodeIds = new Dictionary<string, string>();
                var chainNodes = new List<string>();
                var chainRelations = new List<(string Source, string Target, string Label)>();
                var visitedInChain = new HashSet<string>();

                CollectChainData(analysisResult, handler.FullName, commandType, "Handle", 
                    chainNodes, chainRelations, visitedInChain, chainNodeIds, ref globalNodeCounter);

                chainGroups.Add((chainName, chainNodes, chainRelations, chainNodeIds));
                chainIndex++;
            }
        }

        // 添加子图分组
        for (int i = 0; i < chainGroups.Count; i++)
        {
            var (chainName, chainNodes, _, chainNodeIds) = chainGroups[i];
            
            sb.AppendLine($"    subgraph SG{i + 1} [\"{EscapeMermaidText(chainName)}\"]");
            
            // 添加该链路的所有节点，使用链路独有的节点ID
            foreach (var nodeType in chainNodes)
            {
                var nodeId = GetChainNodeId(nodeType, GetNodeTypeFromFullName(nodeType), chainNodeIds, ref globalNodeCounter);
                AddMultiChainNode(sb, nodeType, nodeId, analysisResult, "        ");
            }
            
            sb.AppendLine("    end");
            sb.AppendLine();
        }

        // 添加链路内部的关系
        sb.AppendLine("    %% Chain Internal Relationships");
        for (int i = 0; i < chainGroups.Count; i++)
        {
            var (_, _, chainRelations, chainNodeIds) = chainGroups[i];
            
            foreach (var (source, target, label) in chainRelations)
            {
                var sourceNodeId = FindChainNodeId(chainNodeIds, source);
                var targetNodeId = FindChainNodeId(chainNodeIds, target);
                
                if (!string.IsNullOrEmpty(sourceNodeId) && !string.IsNullOrEmpty(targetNodeId))
                {
                    var arrow = GetArrowStyle(GetRelationTypeFromLabel(label));
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
        }

        sb.AppendLine();
        AddMultiChainStyles(sb);

        return sb.ToString();
    }

    /// <summary>
    /// 收集单个链路的数据（节点和关系）
    /// </summary>
    private static void CollectChainData(CodeFlowAnalysisResult analysisResult, string startType, string commandType, 
        string startMethod, List<string> chainNodes, List<(string Source, string Target, string Label)> chainRelations, 
        HashSet<string> visitedInChain, Dictionary<string, string> chainNodeIds, ref int globalNodeCounter)
    {
        if (visitedInChain.Contains(startType))
            return;

        // 添加起点
        chainNodes.Add(startType);
        visitedInChain.Add(startType);

        // 添加命令
        if (!visitedInChain.Contains(commandType))
        {
            chainNodes.Add(commandType);
            visitedInChain.Add(commandType);
        }

        // 添加起点到命令的关系
        chainRelations.Add((startType, commandType, startMethod));

        // 跟踪命令执行链路
        TraceChainExecution(analysisResult, commandType, chainNodes, chainRelations, visitedInChain);
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
            }

            var integrationEventType = converter.IntegrationEventType;
            if (!visitedInChain.Contains(integrationEventType))
            {
                chainNodes.Add(integrationEventType);
                visitedInChain.Add(integrationEventType);
                
                chainRelations.Add((converter.FullName, integrationEventType, "to"));

                // 跟踪集成事件处理器
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

    /// <summary>
    /// 添加多链路图中的节点
    /// </summary>
    private static void AddMultiChainNode(StringBuilder sb, string nodeType, string nodeId, 
        CodeFlowAnalysisResult analysisResult, string indent)
    {
        var nodeName = GetClassNameFromFullName(nodeType);

        // 根据节点类型确定样式
        var controller = analysisResult.Controllers.FirstOrDefault(c => c.FullName == nodeType);
        var command = analysisResult.Commands.FirstOrDefault(c => c.FullName == nodeType);
        var entity = analysisResult.Entities.FirstOrDefault(e => e.FullName == nodeType);
        var domainEvent = analysisResult.DomainEvents.FirstOrDefault(d => d.FullName == nodeType);
        var integrationEvent = analysisResult.IntegrationEvents.FirstOrDefault(i => i.FullName == nodeType);

        if (controller != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else if (command != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else if (entity != null)
        {
            var shape = entity.IsAggregateRoot ? "{{" + EscapeMermaidText(nodeName) + "}}" : "[" + EscapeMermaidText(nodeName) + "]";
            sb.AppendLine($"{indent}{nodeId}{shape}");
        }
        else if (domainEvent != null)
        {
            sb.AppendLine($"{indent}{nodeId}(\"{EscapeMermaidText(nodeName)}\")");
        }
        else if (integrationEvent != null)
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
        else
        {
            sb.AppendLine($"{indent}{nodeId}[\"{EscapeMermaidText(nodeName)}\"]");
        }
    }

    /// <summary>
    /// 从标签推断关系类型
    /// </summary>
    private static string GetRelationTypeFromLabel(string label)
    {
        return label switch
        {
            var l when l.Contains("executes") => "CommandToAggregateMethod",
            var l when l.Contains("handles") => "DomainEventToHandler",
            var l when l.Contains("converts") => "DomainEventToIntegrationEvent",
            var l when l.Contains("sends") => "HandlerToCommand",
            _ => "MethodToCommand"
        };
    }

    /// <summary>
    /// 添加多链路图样式
    /// </summary>
    private static void AddMultiChainStyles(StringBuilder sb)
    {
        sb.AppendLine("    %% Multi-Chain Styles");
        sb.AppendLine("    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:2px;");
        sb.AppendLine("    classDef command fill:#f3e5f5,stroke:#4a148c,stroke-width:2px;");
        sb.AppendLine("    classDef entity fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px;");
        sb.AppendLine("    classDef domainEvent fill:#fff3e0,stroke:#e65100,stroke-width:2px;");
        sb.AppendLine("    classDef integrationEvent fill:#fce4ec,stroke:#880e4f,stroke-width:2px;");
        sb.AppendLine("    classDef handler fill:#f1f8e9,stroke:#33691e,stroke-width:2px;");
        sb.AppendLine("    classDef converter fill:#e3f2fd,stroke:#0277bd,stroke-width:2px;");
        sb.AppendLine();
        
        // 应用样式到节点
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

    /// <summary>
    /// 为链路中的节点获取唯一的ID
    /// </summary>
    private static string GetChainNodeId(string fullName, string nodeType, Dictionary<string, string> chainNodeIds, ref int globalNodeCounter)
    {
        var key = $"{nodeType}_{fullName}";
        if (!chainNodeIds.ContainsKey(key))
        {
            chainNodeIds[key] = $"{nodeType}{globalNodeCounter++}";
        }
        return chainNodeIds[key];
    }

    /// <summary>
    /// 在链路的节点ID映射中查找节点ID
    /// </summary>
    private static string FindChainNodeId(Dictionary<string, string> chainNodeIds, string fullName)
    {
        foreach (var kvp in chainNodeIds)
        {
            if (kvp.Key.EndsWith($"_{fullName}"))
            {
                return kvp.Value;
            }
        }
        return string.Empty;
    }

    #endregion
}
