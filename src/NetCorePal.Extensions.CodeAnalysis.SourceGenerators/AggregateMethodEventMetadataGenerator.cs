using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Generator]
public class AggregateMethodEventMetadataGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(c => c != null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
        {
            var (compilation, classes) = source;
            var aggregates = new List<(string AggregateType, string MethodName, string[] EventTypes)>();

            // 定义递归事件收集结构
            var methodCallGraph = new Dictionary<(string, string), List<(string, string)>>(); // (type, method) -> List<(type, method)>
            var methodDirectEvents = new Dictionary<(string, string), List<string>>(); // (type, method) -> List<eventType>

            foreach (var classDecl in classes)
            {
                var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                var symbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol == null) continue;
                if (!symbol.AllInterfaces.Any(i => i.Name == "IAggregateRoot")) continue;
                var aggregateType = symbol.ToDisplayString();
                foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) continue;
                    var methodName = method.Identifier.Text;
                    var methodKey = (aggregateType, methodName);
                    if (!methodCallGraph.ContainsKey(methodKey)) methodCallGraph[methodKey] = new List<(string, string)>();
                    if (!methodDirectEvents.ContainsKey(methodKey)) methodDirectEvents[methodKey] = new List<string>();

                    // 收集方法体内的调用
                    var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
                    foreach (var invocation in invocations)
                    {
                        // 方法调用
                        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                        {
                            var calledSymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
                            if (calledSymbol == null) continue;
                            // 收集 AddDomainEvent 直接事件
                            if (memberAccess.Name.Identifier.Text == "AddDomainEvent")
                            {
                                var arg = invocation.ArgumentList.Arguments.FirstOrDefault();
                                if (arg != null)
                                {
                                    var eventTypeSymbol = semanticModel.GetTypeInfo(arg.Expression).Type;
                                    if (eventTypeSymbol == null && arg.Expression is ObjectCreationExpressionSyntax objCreation)
                                        eventTypeSymbol = semanticModel.GetTypeInfo(objCreation).Type;
                                    if (eventTypeSymbol != null && IsDomainEventType(eventTypeSymbol))
                                    {
                                        var eventType = eventTypeSymbol.ToDisplayString();
                                        if (!string.IsNullOrEmpty(eventType))
                                            methodDirectEvents[methodKey].Add(eventType);
                                    }
                                }
                            }
                            // 收集本类型下的方法调用
                            else if (calledSymbol.ContainingType.ToDisplayString() == aggregateType)
                            {
                                var calleeKey = (aggregateType, calledSymbol.Name);
                                methodCallGraph[methodKey].Add(calleeKey);
                            }
                        }
                    }
                }
            }

            // 递归收集所有下游事件
            HashSet<string> CollectAllEvents((string, string) method, HashSet<(string, string)>? visited = null)
            {
                visited ??= new HashSet<(string, string)>();
                if (visited.Contains(method)) return new HashSet<string>();
                visited.Add(method);
                var events = new HashSet<string>();
                if (methodDirectEvents.TryGetValue(method, out var directEvents))
                    events.UnionWith(directEvents);
                if (methodCallGraph.TryGetValue(method, out var callees))
                {
                    foreach (var callee in callees)
                        events.UnionWith(CollectAllEvents(callee, visited));
                }
                return events;
            }

            // 输出 Attribute
            foreach (var classDecl in classes)
            {
                var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                var symbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol == null) continue;
                if (!symbol.AllInterfaces.Any(i => i.Name == "IAggregateRoot")) continue;
                var aggregateType = symbol.ToDisplayString();
                foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) continue;
                    var methodName = method.Identifier.Text;
                    var methodKey = (aggregateType, methodName);
                    var allEvents = CollectAllEvents(methodKey);
                    aggregates.Add((aggregateType, methodName, allEvents.ToArray()));
                }
            }

            if (aggregates.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated/>\nusing System;\nusing NetCorePal.Extensions.CodeAnalysis.Attributes;");
                foreach (var agg in aggregates)
                {
                    if (agg.EventTypes.Length > 0)
                    {
                        var events = string.Join(", ", agg.EventTypes.Select(e => $"\"{e}\""));
                        sb.AppendLine($"[assembly: AggregateMethodEventMetadataAttribute(\"{agg.AggregateType}\", \"{agg.MethodName}\", {events})]\n");
                    }
                    else
                    {
                        sb.AppendLine($"[assembly: AggregateMethodEventMetadataAttribute(\"{agg.AggregateType}\", \"{agg.MethodName}\")]\n");
                    }
                }
                spc.AddSource("AggregateMethodEventMetadata.g.cs", sb.ToString());
            }

            // 辅助方法：判断类型是否为领域事件
            bool IsDomainEventType(ITypeSymbol typeSymbol)
            {
                if (typeSymbol is INamedTypeSymbol namedType)
                {
                    return namedType.AllInterfaces.Any(i => i.Name == "IDomainEvent");
                }
                return false;
            }
        });
    }
} 