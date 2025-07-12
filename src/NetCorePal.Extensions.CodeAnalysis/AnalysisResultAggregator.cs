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
        var allResults = new List<CodeFlowAnalysisResult>();

        // 扫描所有程序集，查找实现了 IAnalysisResult 的类型
        foreach (var assembly in assemblies)
        {
            try
            {
                var analysisResultTypes = assembly.GetTypes()
                    .Where(type => typeof(IAnalysisResult).IsAssignableFrom(type) 
                                   && !type.IsInterface 
                                   && !type.IsAbstract
                                   && type.GetConstructor(Type.EmptyTypes) != null)
                    .ToList();

                // 实例化并获取分析结果
                foreach (var type in analysisResultTypes)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(type!) as IAnalysisResult;
                        var result = instance?.GetResult();
                        if (result != null)
                        {
                            allResults.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 忽略无法实例化的类型
                        // 可以考虑添加日志记录
                        Console.WriteLine($"Warning: Failed to create instance of {type.FullName}: {ex.Message}");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 处理程序集加载异常，只处理能够加载的类型
                var loadedTypes = ex.Types.Where(t => t != null);
                var analysisResultTypes = loadedTypes
                    .Where(type => typeof(IAnalysisResult).IsAssignableFrom(type) 
                                   && !type.IsInterface 
                                   && !type.IsAbstract
                                   && type.GetConstructor(Type.EmptyTypes) != null)
                    .ToList();

                foreach (var type in analysisResultTypes)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(type!) as IAnalysisResult;
                        var result = instance?.GetResult();
                        if (result != null)
                        {
                            allResults.Add(result);
                        }
                    }
                    catch (Exception instanceEx)
                    {
                        Console.WriteLine($"Warning: Failed to create instance of {type?.FullName ?? "Unknown"}: {instanceEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to scan assembly {assembly.FullName}: {ex.Message}");
            }
        }

        // 合并所有分析结果
        MergeResults(aggregatedResult, allResults);

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
}
