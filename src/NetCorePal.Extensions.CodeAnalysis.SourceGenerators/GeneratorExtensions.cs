using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetCorePal.Extensions.CodeAnalysis;

public static class GeneratorExtensions
{
    /// <summary>
    /// 判断类型是否为命令处理器（实现了ICommandHandler接口）
    /// </summary>
    public static bool IsCommandHandler(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(i =>
            (i.Name == "ICommandHandler" && i.TypeArguments.Length == 1) ||
            (i.Name == "ICommandHandler" && i.TypeArguments.Length == 2)
        );
    }
    /// <summary>
    /// 判断类型是否为聚合根（实现了IAggregateRoot接口）
    /// </summary>
    public static bool IsAggregateRoot(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(i => i.Name == "IAggregateRoot");
    }

    /// <summary>
    /// 判断类型是否为领域事件（实现了IDomainEvent接口）
    /// </summary>
    public static bool IsDomainEvent(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(i => i.Name == "IDomainEvent");
    }

    /// <summary>
    /// 判断类型是否为命令（实现了ICommand接口）
    /// </summary>
    public static bool IsCommand(this INamedTypeSymbol typeSymbol)
    {
        // 判断是否实现了 ICommand 或泛型 ICommand<>
        return typeSymbol.AllInterfaces.Any(i =>
            i.Name == "ICommand" && (i.TypeArguments.Length == 0 || i.TypeArguments.Length == 1)
        );
    }

    /// <summary>
    /// 判断调用是否是 AddDomainEvent，如果是则通过 out 返回领域事件类型，否则返回 false
    /// </summary>
    public static bool IsAddDomainEventInvocation(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, out INamedTypeSymbol? eventType)
    {
        eventType = null;
        if ((invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "AddDomainEvent") ||
            (invocation.Expression is IdentifierNameSyntax identifierName && identifierName.Identifier.Text == "AddDomainEvent"))
        {
            var arg = invocation.ArgumentList.Arguments.FirstOrDefault();
            if (arg != null)
            {
                var eventTypeSymbol = semanticModel.GetTypeInfo(arg.Expression).Type as INamedTypeSymbol;
                if (eventTypeSymbol != null && eventTypeSymbol.IsDomainEvent())
                {
                    eventType = eventTypeSymbol;
                    return true;
                }
                // fallback: symbol info
                var fallbackSymbol = semanticModel.GetSymbolInfo(arg.Expression).Symbol as INamedTypeSymbol;
                if (fallbackSymbol != null && fallbackSymbol.IsDomainEvent())
                {
                    eventType = fallbackSymbol;
                    return true;
                }
            }
        }
        return false;
    }
} 