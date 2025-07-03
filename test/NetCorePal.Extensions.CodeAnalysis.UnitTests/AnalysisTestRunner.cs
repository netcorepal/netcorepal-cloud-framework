using System;
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

/// <summary>
/// 测试运行器，用于在控制台输出分析结果
/// </summary>
public class AnalysisTestRunner
{
    public static void RunAnalysis()
    {
        Console.WriteLine("开始运行代码分析...");
        Console.WriteLine("===================================");

        var assembly = Assembly.GetExecutingAssembly();
        var result = AnalysisResultAggregator.Aggregate(assembly);

        Console.WriteLine($"发现 {result.Controllers.Count} 个控制器:");
        foreach (var controller in result.Controllers)
        {
            Console.WriteLine($"  - {controller.Name} ({controller.FullName})");
            foreach (var method in controller.Methods)
            {
                Console.WriteLine($"    方法: {method}");
            }
        }

        Console.WriteLine($"\n发现 {result.Commands.Count} 个命令:");
        foreach (var command in result.Commands)
        {
            Console.WriteLine($"  - {command.Name} ({command.FullName})");
        }

        Console.WriteLine($"\n发现 {result.Entities.Count} 个实体/聚合:");
        foreach (var entity in result.Entities)
        {
            Console.WriteLine($"  - {entity.Name} ({entity.FullName}) [聚合根: {entity.IsAggregateRoot}]");
            foreach (var method in entity.Methods)
            {
                Console.WriteLine($"    方法: {method}");
            }
        }

        Console.WriteLine($"\n发现 {result.DomainEvents.Count} 个领域事件:");
        foreach (var domainEvent in result.DomainEvents)
        {
            Console.WriteLine($"  - {domainEvent.Name} ({domainEvent.FullName})");
        }

        Console.WriteLine($"\n发现 {result.DomainEventHandlers.Count} 个领域事件处理器:");
        foreach (var handler in result.DomainEventHandlers)
        {
            Console.WriteLine($"  - {handler.Name} ({handler.FullName})");
            Console.WriteLine($"    处理事件: {handler.HandledEventType}");
        }

        Console.WriteLine($"\n发现 {result.IntegrationEvents.Count} 个集成事件:");
        foreach (var integrationEvent in result.IntegrationEvents)
        {
            Console.WriteLine($"  - {integrationEvent.Name} ({integrationEvent.FullName})");
        }

        Console.WriteLine($"\n发现 {result.IntegrationEventHandlers.Count} 个集成事件处理器:");
        foreach (var handler in result.IntegrationEventHandlers)
        {
            Console.WriteLine($"  - {handler.Name} ({handler.FullName})");
            Console.WriteLine($"    处理事件: {handler.HandledEventType}");
        }

        Console.WriteLine($"\n发现 {result.IntegrationEventConverters.Count} 个集成事件转换器:");
        foreach (var converter in result.IntegrationEventConverters)
        {
            Console.WriteLine($"  - {converter.Name} ({converter.FullName})");
            Console.WriteLine($"    转换: {converter.DomainEventType} -> {converter.IntegrationEventType}");
        }

        Console.WriteLine($"\n发现 {result.Relationships.Count} 个关系:");
        foreach (var relationship in result.Relationships)
        {
            Console.WriteLine($"  - {relationship.SourceType}.{relationship.SourceMethod} -> {relationship.TargetType}.{relationship.TargetMethod} ({relationship.CallType})");
        }

        Console.WriteLine("\n===================================");
        Console.WriteLine("代码分析完成!");
    }
}
