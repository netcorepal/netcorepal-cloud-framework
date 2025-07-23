using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 分析结果聚合器，用于扫描程序集并合并分析结果
/// </summary>
public static class AnalysisResultAggregator
{
    /// <summary>
    /// 扫描指定程序集中实现了 IAnalysisResult 的类型，并合并所有分析结果
    /// </summary>
    /// <param name="assemblies">要扫描的程序集列表</param>
    /// <returns>合并后的代码流分析结果</returns>
    public static CodeFlowAnalysisResult Aggregate(params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            return new CodeFlowAnalysisResult();
        }

        var aggregatedResult = new CodeFlowAnalysisResult();

        foreach (var assembly in assemblies)
        {
            try
            {
                // 命令元数据收集（放在最前，避免影响后续Attribute处理）
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.CommandMetadataAttribute), false)
                    .Cast<Attributes.CommandMetadataAttribute>())
                {
                    if (!aggregatedResult.Commands.Any(c => c.FullName == attr.CommandType))
                    {
                        aggregatedResult.Commands.Add(new CommandInfo { Name = GetClassNameFromFullName(attr.CommandType), FullName = attr.CommandType, Properties = new List<string>() });
                    }
                }
                // Controller → Command
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.ControllerMetadataAttribute), false)
                    .Cast<Attributes.ControllerMetadataAttribute>())
                {
                    var controller = aggregatedResult.Controllers.FirstOrDefault(c => c.FullName == attr.ControllerType);
                    if (controller == null)
                    {
                        controller = new ControllerInfo { Name = GetClassNameFromFullName(attr.ControllerType), FullName = attr.ControllerType };
                        aggregatedResult.Controllers.Add(controller);
                    }
                    if (!controller.Methods.Contains(attr.ControllerMethodName))
                        controller.Methods.Add(attr.ControllerMethodName);
                    // 关系
                    foreach (var cmd in attr.CommandTypes)
                    {
                        aggregatedResult.Relationships.Add(new CallRelationship(attr.ControllerType, attr.ControllerMethodName, cmd, "", "MethodToCommand"));
                    }
                }
                // CommandSender → Command
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.CommandSenderMetadataAttribute), false)
                    .Cast<Attributes.CommandSenderMetadataAttribute>())
                {
                    var sender = aggregatedResult.CommandSenders.FirstOrDefault(s => s.FullName == attr.SenderType);
                    if (sender == null)
                    {
                        sender = new CommandSenderInfo { Name = GetClassNameFromFullName(attr.SenderType), FullName = attr.SenderType };
                        aggregatedResult.CommandSenders.Add(sender);
                    }
                    if (!sender.Methods.Contains(attr.SenderMethodName))
                        sender.Methods.Add(attr.SenderMethodName);
                    foreach (var cmd in attr.CommandTypes)
                    {
                        aggregatedResult.Relationships.Add(new CallRelationship(attr.SenderType, attr.SenderMethodName, cmd, "", "MethodToCommand"));
                    }
                }
                // 聚合实体元数据
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.EntityMetadataAttribute), false)
                    .Cast<Attributes.EntityMetadataAttribute>())
                {
                    var entity = aggregatedResult.Entities.FirstOrDefault(e => e.FullName == attr.EntityType);
                    if (entity == null)
                    {
                        entity = new EntityInfo { Name = GetClassNameFromFullName(attr.EntityType), FullName = attr.EntityType, IsAggregateRoot = attr.IsAggregateRoot };
                        aggregatedResult.Entities.Add(entity);
                    }
                    foreach (var method in attr.MethodNames)
                    {
                        if (!entity.Methods.Contains(method))
                            entity.Methods.Add(method);
                    }
                }
                // 聚合方法 → 领域事件
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.EntityMethodMetadataAttribute), false)
                    .Cast<Attributes.EntityMethodMetadataAttribute>())
                {
                    var entity = aggregatedResult.Entities.FirstOrDefault(e => e.FullName == attr.EntityType);
                    if (entity == null)
                    {
                        entity = new EntityInfo { Name = GetClassNameFromFullName(attr.EntityType), FullName = attr.EntityType, IsAggregateRoot = true };
                        aggregatedResult.Entities.Add(entity);
                    }
                    if (!entity.Methods.Contains(attr.MethodName))
                        entity.Methods.Add(attr.MethodName);
                    foreach (var evt in attr.EventTypes)
                    {
                        if (!aggregatedResult.DomainEvents.Any(e => e.FullName == evt))
                            aggregatedResult.DomainEvents.Add(new DomainEventInfo { Name = GetClassNameFromFullName(evt), FullName = evt });
                        aggregatedResult.Relationships.Add(new CallRelationship(attr.EntityType, attr.MethodName, evt, "", "MethodToDomainEvent"));
                    }
                }
                // 领域事件 → 集成事件
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.IntegrationEventConverterMetadataAttribute), false)
                    .Cast<Attributes.IntegrationEventConverterMetadataAttribute>())
                {
                    if (!aggregatedResult.DomainEvents.Any(e => e.FullName == attr.DomainEventType))
                        aggregatedResult.DomainEvents.Add(new DomainEventInfo { Name = GetClassNameFromFullName(attr.DomainEventType), FullName = attr.DomainEventType });
                    if (!aggregatedResult.IntegrationEvents.Any(e => e.FullName == attr.IntegrationEventType))
                        aggregatedResult.IntegrationEvents.Add(new IntegrationEventInfo { Name = GetClassNameFromFullName(attr.IntegrationEventType), FullName = attr.IntegrationEventType });
                    aggregatedResult.Relationships.Add(new CallRelationship(attr.DomainEventType, "", attr.IntegrationEventType, "", "DomainEventToIntegrationEvent"));
                    // 新增 IntegrationEventConverterInfo
                    if (!aggregatedResult.IntegrationEventConverters.Any(c =>
                        c.DomainEventType == attr.DomainEventType && c.IntegrationEventType == attr.IntegrationEventType))
                    {
                        aggregatedResult.IntegrationEventConverters.Add(new IntegrationEventConverterInfo
                        {
                            Name = $"{GetClassNameFromFullName(attr.DomainEventType)}To{GetClassNameFromFullName(attr.IntegrationEventType)}Converter",
                            FullName = $"{attr.DomainEventType}To{attr.IntegrationEventType}Converter",
                            DomainEventType = attr.DomainEventType,
                            IntegrationEventType = attr.IntegrationEventType
                        });
                    }
                }
                // 领域事件处理器 → 命令
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.DomainEventHandlerMetadataAttribute), false)
                    .Cast<Attributes.DomainEventHandlerMetadataAttribute>())
                {
                    var handler = aggregatedResult.DomainEventHandlers.FirstOrDefault(h => h.FullName == attr.HandlerType);
                    if (handler == null)
                    {
                        handler = new DomainEventHandlerInfo { Name = GetClassNameFromFullName(attr.HandlerType), FullName = attr.HandlerType, HandledEventType = attr.EventType };
                        aggregatedResult.DomainEventHandlers.Add(handler);
                    }
                    foreach (var cmd in attr.CommandTypes)
                    {
                        if (!handler.Commands.Contains(cmd))
                            handler.Commands.Add(cmd);
                        aggregatedResult.Relationships.Add(new CallRelationship(attr.HandlerType, "", cmd, "", "HandlerToCommand"));
                    }
                }
                // 集成事件处理器 → 命令
                foreach (var attr in assembly.GetCustomAttributes(typeof(Attributes.IntegrationEventHandlerMetadataAttribute), false)
                    .Cast<Attributes.IntegrationEventHandlerMetadataAttribute>())
                {
                    var handler = aggregatedResult.IntegrationEventHandlers.FirstOrDefault(h => h.FullName == attr.HandlerType);
                    if (handler == null)
                    {
                        handler = new IntegrationEventHandlerInfo { Name = GetClassNameFromFullName(attr.HandlerType), FullName = attr.HandlerType, HandledEventType = attr.EventType };
                        aggregatedResult.IntegrationEventHandlers.Add(handler);
                    }
                    foreach (var cmd in attr.CommandTypes)
                    {
                        if (!handler.Commands.Contains(cmd))
                            handler.Commands.Add(cmd);
                        aggregatedResult.Relationships.Add(new CallRelationship(attr.HandlerType, "", cmd, "", "HandlerToCommand"));
                    }
                }
                // 集成事件转换器
                // 这里暂不处理 IntegrationEventConverterInfo，因 Attribute 结构未直接对应
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to scan attributes in assembly {assembly.FullName}: {ex.Message}");
            }
        }

        return aggregatedResult;
    }

    /// <summary>
    /// 扫描当前应用程序域中加载的所有程序集，并合并分析结果
    /// </summary>
    /// <returns>合并后的代码流分析结果</returns>
    public static CodeFlowAnalysisResult AggregateFromCurrentDomain()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return Aggregate(assemblies);
    }

    /// <summary>
    /// 扫描指定程序集名称的程序集，并合并分析结果
    /// </summary>
    /// <param name="assemblyNames">程序集名称列表</param>
    /// <returns>合并后的代码流分析结果</returns>
    public static CodeFlowAnalysisResult AggregateFromAssemblyNames(params string[] assemblyNames)
    {
        if (assemblyNames == null || assemblyNames.Length == 0)
        {
            return new CodeFlowAnalysisResult();
        }

        var assemblies = new List<Assembly>();
        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyName);
                assemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load assembly {assemblyName}: {ex.Message}");
            }
        }

        return Aggregate(assemblies.ToArray());
    }

    /// <summary>
    /// 合并多个分析结果，避免重复数据
    /// </summary>
    /// <param name="target">目标结果对象</param>
    /// <param name="sources">源结果列表</param>
    private static void MergeResults(CodeFlowAnalysisResult target, List<CodeFlowAnalysisResult> sources)
    {
        // 使用哈希集合来去重
        var controllerSet = new HashSet<string>();
        var commandSenderSet = new HashSet<string>();
        var commandSet = new HashSet<string>();
        var entitySet = new HashSet<string>();
        var domainEventSet = new HashSet<string>();
        var domainEventHandlerSet = new HashSet<string>();
        var integrationEventSet = new HashSet<string>();
        var integrationEventHandlerSet = new HashSet<string>();
        var integrationEventConverterSet = new HashSet<string>();
        var relationshipSet = new HashSet<string>();

        foreach (var source in sources)
        {
            // 合并控制器
            foreach (var controller in source.Controllers)
            {
                var key = $"{controller.Name}:{controller.FullName}";
                if (controllerSet.Add(key))
                {
                    target.Controllers.Add(controller);
                }
            }

            // 合并命令发送者
            foreach (var commandSender in source.CommandSenders)
            {
                var key = $"{commandSender.Name}:{commandSender.FullName}";
                if (commandSenderSet.Add(key))
                {
                    target.CommandSenders.Add(commandSender);
                }
            }

            // 合并命令
            foreach (var command in source.Commands)
            {
                var key = $"{command.Name}:{command.FullName}";
                if (commandSet.Add(key))
                {
                    target.Commands.Add(command);
                }
            }

            // 合并实体
            foreach (var entity in source.Entities)
            {
                var key = $"{entity.Name}:{entity.FullName}";
                if (entitySet.Add(key))
                {
                    target.Entities.Add(entity);
                }
            }

            // 合并领域事件
            foreach (var domainEvent in source.DomainEvents)
            {
                var key = $"{domainEvent.Name}:{domainEvent.FullName}";
                if (domainEventSet.Add(key))
                {
                    target.DomainEvents.Add(domainEvent);
                }
            }

            // 合并领域事件处理器
            foreach (var handler in source.DomainEventHandlers)
            {
                var key = $"{handler.Name}:{handler.FullName}:{handler.HandledEventType}";
                if (domainEventHandlerSet.Add(key))
                {
                    target.DomainEventHandlers.Add(handler);
                }
            }

            // 合并集成事件
            foreach (var integrationEvent in source.IntegrationEvents)
            {
                var key = $"{integrationEvent.Name}:{integrationEvent.FullName}";
                if (integrationEventSet.Add(key))
                {
                    target.IntegrationEvents.Add(integrationEvent);
                }
            }

            // 合并集成事件处理器
            foreach (var handler in source.IntegrationEventHandlers)
            {
                var key = $"{handler.Name}:{handler.FullName}:{handler.HandledEventType}";
                if (integrationEventHandlerSet.Add(key))
                {
                    target.IntegrationEventHandlers.Add(handler);
                }
            }

            // 合并集成事件转换器
            foreach (var converter in source.IntegrationEventConverters)
            {
                var key = $"{converter.Name}:{converter.FullName}:{converter.DomainEventType}:{converter.IntegrationEventType}";
                if (integrationEventConverterSet.Add(key))
                {
                    target.IntegrationEventConverters.Add(converter);
                }
            }

            // 合并调用关系
            foreach (var relationship in source.Relationships)
            {
                var key = $"{relationship.SourceType}:{relationship.SourceMethod}:{relationship.TargetType}:{relationship.TargetMethod}:{relationship.CallType}";
                if (relationshipSet.Add(key))
                {
                    target.Relationships.Add(relationship);
                }
            }
        }
    }

    private static string GetClassNameFromFullName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        var parts = fullName.Split('.');
        return parts.LastOrDefault() ?? "";
    }
}
