using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace NetCorePal.Extensions.CodeAnalysis.SourceGenerators
{
    public static class GeneratorExtensions
    {
        public static IEnumerable<string> GetSentCommandTypes(this MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            var result = new HashSet<string>();
            if (method.Body == null) return result;

            foreach (var invocation in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                // 只处理 .Send 调用
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "Send")
                {
                    // 获取调用者类型
                    var exprSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                    INamedTypeSymbol? mediatorType = null;
                    if (exprSymbol is ILocalSymbol local)
                        mediatorType = local.Type as INamedTypeSymbol;
                    else if (exprSymbol is IFieldSymbol field)
                        mediatorType = field.Type as INamedTypeSymbol;
                    else if (exprSymbol is IPropertySymbol prop)
                        mediatorType = prop.Type as INamedTypeSymbol;
                    else if (exprSymbol is IParameterSymbol param)
                        mediatorType = param.Type as INamedTypeSymbol;
                    // 必须实现 IMediator 或自身就是 IMediator
                    if (mediatorType != null && (mediatorType.Name == "IMediator" || mediatorType.AllInterfaces.Any(i => i.Name == "IMediator")))
                    {
                        // 检查第一个参数是否命令
                        var arg = invocation.ArgumentList.Arguments.FirstOrDefault();
                        if (arg != null)
                        {
                            var typeInfo = semanticModel.GetTypeInfo(arg.Expression);
                            if (typeInfo.Type is INamedTypeSymbol typeSymbol && typeSymbol.IsCommand())
                            {
                                result.Add(typeSymbol.ToDisplayString());
                            }
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 判断调用是否为实体方法调用，如果是，out 返回 (实体类型名, 方法名)，否则 out (null, null) 并返回 false。
        /// </summary>
        public static bool IsEntityMethodInvocation(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, out (string? entityType, string? methodName) result)
        {
            result = (null, null);
            var invokedSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (invokedSymbol != null)
            {
                var containingType = invokedSymbol.ContainingType;
                if (containingType != null && containingType.IsEntity())
                {
                    result = (containingType.ToDisplayString(), invokedSymbol.Name);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断类型是否为实体（继承自Entity或Entity&lt;TKey&gt;）
        /// </summary>
        public static bool IsEntity(this INamedTypeSymbol typeSymbol)
        {
            // 检查基类是否为 Entity 或泛型 Entity<>
            var baseType = typeSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "Entity" && baseType.ContainingNamespace.ToDisplayString().Contains("NetCorePal.Extensions.Domain"))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
        /// <summary>
        /// 判断类型是否为聚合根或实体（实现了IAggregateRoot或IEntity接口）
        /// </summary>
        public static bool IsAggregateOrEntity(this INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.IsAggregateRoot() || typeSymbol.AllInterfaces.Any(i => i.Name == "IEntity");
        }

        

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
}