using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace NetCorePal.Extensions.CodeAnalysis.SourceGenerators
{
    public static class GeneratorExtensions
    {
        /// <summary>
        /// Checks if type implements IDomainEventHandler&lt;TEvent&gt; or IDomainEventHandler&lt;TEvent, TResponse&gt; or IRequestHandler&lt;TEvent&gt; or IRequestHandler&lt;TEvent, TResponse&gt;
        /// but not ICommandHandler (to avoid matching command handlers that inherit from IRequestHandler)
        /// </summary>
        public static bool IsDomainEventHandler(this INamedTypeSymbol typeSymbol)
        {
            // First check if it's a command handler - if so, it's not a domain event handler
            if (typeSymbol.IsCommandHandler()) return false;
            
            return typeSymbol.AllInterfaces.Any(i => 
                (i.Name == "IDomainEventHandler" && (i.TypeArguments.Length == 1 || i.TypeArguments.Length == 2)) ||
                (i.Name == "IRequestHandler" && (i.TypeArguments.Length == 1 || i.TypeArguments.Length == 2))
            );
        }

        /// <summary>
        /// 从领域事件处理器类型中获取领域事件类型
        /// </summary>
        /// <param name="typeSymbol">领域事件处理器类型</param>
        /// <returns>领域事件类型，如果找不到则返回null</returns>
        public static ITypeSymbol? GetDomainEventFromDomainEventHandler(this INamedTypeSymbol typeSymbol)
        {
            var handlerInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => 
                (i.Name == "IDomainEventHandler" || i.Name == "IRequestHandler") && 
                i.TypeArguments.Length >= 1);
            
            return handlerInterface?.TypeArguments[0];
        }

        /// <summary>
        /// 从集成事件处理器类型中获取集成事件类型
        /// </summary>
        /// <param name="typeSymbol">集成事件处理器类型</param>
        /// <returns>集成事件类型，如果找不到则返回null</returns>
        public static ITypeSymbol? GetIntegrationEventFromIntegrationEventHandler(this INamedTypeSymbol typeSymbol)
        {
            var handlerInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => 
                i.Name == "IIntegrationEventHandler" && 
                i.TypeArguments.Length == 1);
            
            return handlerInterface?.TypeArguments[0];
        }

        /// <summary>
        /// 从命令处理器类型中获取命令类型
        /// </summary>
        /// <param name="typeSymbol">命令处理器类型</param>
        /// <returns>命令类型，如果找不到则返回null</returns>
        public static ITypeSymbol? GetCommandFromCommandHandler(this INamedTypeSymbol typeSymbol)
        {
            // 首先检查是否实现了 ICommandHandler 接口
            var commandHandlerInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => 
                i.Name == "ICommandHandler" && 
                (i.TypeArguments.Length == 1 || i.TypeArguments.Length == 2));
            
            if (commandHandlerInterface != null)
            {
                return commandHandlerInterface.TypeArguments[0];
            }
            
            // 如果没有直接实现 ICommandHandler，检查是否实现了 IRequestHandler
            // 但需要确保第一个类型参数是命令类型（实现了 ICommand 接口）
            var requestHandlerInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => 
                i.Name == "IRequestHandler" && 
                (i.TypeArguments.Length == 1 || i.TypeArguments.Length == 2));
            
            if (requestHandlerInterface != null)
            {
                var firstTypeArg = requestHandlerInterface.TypeArguments[0];
                // 检查第一个类型参数是否为命令类型
                if (firstTypeArg is INamedTypeSymbol namedType && namedType.IsCommand())
                {
                    return firstTypeArg;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 判断类型是否为集成事件处理器（实现了 IIntegrationEventHandler&lt;TEvent&gt; 接口）
        /// </summary>
        public static bool IsIntegrationEventHandler(this INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.AllInterfaces.Any(i => i.Name == "IIntegrationEventHandler" && i.TypeArguments.Length == 1);
        }

        /// <summary>
        /// 判断类型是否为集成事件转换器（实现了 IIntegrationEventConverter&lt;TDomainEvent, TIntegrationEvent&gt; 接口）
        /// </summary>
        public static bool IsIntegrationEventConverter(this INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.AllInterfaces.Any(i => i.Name == "IIntegrationEventConverter" && i.TypeArguments.Length == 2);
        }

        /// <summary>
        /// 判断类型是否为 Endpoint（支持 FastEndpoints 基类及泛型）
        /// </summary>
        public static bool IsEndpoint(this INamedTypeSymbol typeSymbol)
        {
            var allTypes = typeSymbol.AllInterfaces.Concat(new[] { typeSymbol.BaseType });
            foreach (var baseType in allTypes)
            {
                if (baseType == null) continue;
                var baseName = baseType.OriginalDefinition?.ToDisplayString() ?? baseType.ToDisplayString();
                if (baseName == "FastEndpoints.Endpoint" ||
                    baseName.StartsWith("FastEndpoints.Endpoint<") ||
                    baseName == "FastEndpoints.EndpointWithoutRequest" ||
                    baseName.StartsWith("FastEndpoints.EndpointWithoutRequest<"))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断类型是否为 Controller（类名以 Controller 结尾或命名空间包含 Controllers）
        /// </summary>
        public static bool IsController(this INamedTypeSymbol typeSymbol)
        {
            var name = typeSymbol.Name;
            var ns = typeSymbol.ContainingNamespace.ToDisplayString();
            return name.EndsWith("Controller") || ns.Contains("Controllers");
        }
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
        /// 判断类型是否为命令处理器（实现了ICommandHandler接口或实现了IRequestHandler且第一个类型参数为命令）
        /// </summary>
        public static bool IsCommandHandler(this INamedTypeSymbol typeSymbol)
        {
            // 首先检查是否直接实现了 ICommandHandler 接口
            if (typeSymbol.AllInterfaces.Any(i =>
                (i.Name == "ICommandHandler" && i.TypeArguments.Length == 1) ||
                (i.Name == "ICommandHandler" && i.TypeArguments.Length == 2)))
            {
                return true;
            }
            
            // 检查是否实现了 IRequestHandler 且第一个类型参数是命令类型
            var requestHandlerInterface = typeSymbol.AllInterfaces.FirstOrDefault(i => 
                i.Name == "IRequestHandler" && 
                (i.TypeArguments.Length == 1 || i.TypeArguments.Length == 2));
            
            if (requestHandlerInterface != null)
            {
                var firstTypeArg = requestHandlerInterface.TypeArguments[0];
                if (firstTypeArg is INamedTypeSymbol namedType && namedType.IsCommand())
                {
                    return true;
                }
            }
            
            return false;
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