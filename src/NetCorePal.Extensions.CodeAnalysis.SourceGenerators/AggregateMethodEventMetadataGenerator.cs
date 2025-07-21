using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace NetCorePal.Extensions.CodeAnalysis;

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

            // 步骤1：只收集聚合根类型
            var aggregateRootClasses = new List<(INamedTypeSymbol Symbol, ClassDeclarationSyntax Decl, SemanticModel Model)>();
            foreach (var classDecl in classes)
            {
                var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol != null && symbol.IsAggregateRoot())
                {
                    aggregateRootClasses.Add((symbol, classDecl, model));
                }
            }

            // 步骤2：递归收集聚合根所有子实体
            var allEntities = new Dictionary<string, (INamedTypeSymbol Symbol, ClassDeclarationSyntax Decl, SemanticModel Model)>();
            void CollectEntities(INamedTypeSymbol symbol, ClassDeclarationSyntax decl, SemanticModel model)
            {
                var key = symbol.ToDisplayString();
                if (allEntities.ContainsKey(key)) return;
                allEntities[key] = (symbol, decl, model);
                // 属性递归
                foreach (var prop in decl.Members.OfType<PropertyDeclarationSyntax>())
                {
                    var propType = model.GetTypeInfo(prop.Type).Type as INamedTypeSymbol;
                    if (propType != null && IsChildEntityType(propType, symbol))
                    {
                        var childDecl = propType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax;
                        if (childDecl != null)
                        {
                            CollectEntities(propType, childDecl, model);
                        }
                    }
                    else if (propType != null && propType.IsGenericType && propType.TypeArguments.Length == 1)
                    {
                        var elementType = propType.TypeArguments[0] as INamedTypeSymbol;
                        if (elementType != null && IsChildEntityType(elementType, symbol))
                        {
                            var childDecl = elementType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax;
                            if (childDecl != null)
                            {
                                CollectEntities(elementType, childDecl, model);
                            }
                        }
                    }
                }
                // 字段递归
                foreach (var field in decl.Members.OfType<FieldDeclarationSyntax>())
                {
                    var fieldType = model.GetTypeInfo(field.Declaration.Type).Type as INamedTypeSymbol;
                    if (fieldType != null && IsChildEntityType(fieldType, symbol))
                    {
                        var childDecl = fieldType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax;
                        if (childDecl != null)
                        {
                            CollectEntities(fieldType, childDecl, model);
                        }
                    }
                    else if (fieldType != null && fieldType.IsGenericType && fieldType.TypeArguments.Length == 1)
                    {
                        var elementType = fieldType.TypeArguments[0] as INamedTypeSymbol;
                        if (elementType != null && IsChildEntityType(elementType, symbol))
                        {
                            var childDecl = elementType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax;
                            if (childDecl != null)
                            {
                                CollectEntities(elementType, childDecl, model);
                            }
                        }
                    }
                }
            }
            foreach (var (symbol, decl, model) in aggregateRootClasses)
            {
                CollectEntities(symbol, decl, model);
            }

            // 步骤3：收集所有方法、方法与事件的关系、方法之间的调用关系
            var methodDirectEvents = new Dictionary<(string, string), List<string>>(); // (type, methodName) -> List<eventType>
            var methodCallGraph = new Dictionary<(string, string), List<(string, string)>>(); // (type, methodName) -> List<(type, methodName)>
            var allAggregateMethods = new Dictionary<(string, string), string>(); // (type, methodName) -> displayMethodName
            foreach (var (symbol, decl, model) in allEntities.Values)
            {
                var typeName = symbol.ToDisplayString();
                var isAggregateRoot = symbol.IsAggregateRoot();
                var entityPrefix = isAggregateRoot ? null : symbol.Name;
                foreach (var method in decl.Members.OfType<MethodDeclarationSyntax>())
                {
                    var methodName = method.Identifier.Text;
                    // 命名规则：聚合根方法为“方法名”，子实体方法为“子实体.方法名”
                    var displayMethodName = entityPrefix == null ? methodName : $"{entityPrefix}.{methodName}";
                    var methodKey = (typeName, methodName);
                    if (!methodDirectEvents.ContainsKey(methodKey)) methodDirectEvents[methodKey] = new List<string>();
                    if (!methodCallGraph.ContainsKey(methodKey)) methodCallGraph[methodKey] = new List<(string, string)>();
                    if (!allAggregateMethods.ContainsKey(methodKey)) allAggregateMethods[methodKey] = displayMethodName;
                    foreach (var invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        if (invocation.IsAddDomainEventInvocation(model, out var eventTypeSymbol) && eventTypeSymbol != null)
                        {
                            methodDirectEvents[methodKey].Add(eventTypeSymbol.ToDisplayString());
                        }
                        else if (invocation.Expression is MemberAccessExpressionSyntax callMemberAccess)
                        {
                            var calledSymbol = model.GetSymbolInfo(callMemberAccess).Symbol as IMethodSymbol;
                            if (calledSymbol != null)
                            {
                                var calleeType = calledSymbol.ContainingType.ToDisplayString();
                                var calleeKey = (calleeType, calledSymbol.MethodKind == MethodKind.Constructor ? ".ctor" : calledSymbol.Name);
                                methodCallGraph[methodKey].Add(calleeKey);
                            }
                        }
                        // 新增：处理 IdentifierNameSyntax 直接调用
                        else if (invocation.Expression is IdentifierNameSyntax identifierName)
                        {
                            var calledSymbol = model.GetSymbolInfo(identifierName).Symbol as IMethodSymbol;
                            if (calledSymbol != null)
                            {
                                var calleeType = calledSymbol.ContainingType.ToDisplayString();
                                var calleeKey = (calleeType, calledSymbol.MethodKind == MethodKind.Constructor ? ".ctor" : calledSymbol.Name);
                                methodCallGraph[methodKey].Add(calleeKey);
                            }
                        }
                    }
                }
                foreach (var ctor in decl.Members.OfType<ConstructorDeclarationSyntax>())
                {
                    var methodName = ".ctor";
                    var displayMethodName = entityPrefix == null ? methodName : $"{entityPrefix}.{methodName}";
                    var methodKey = (typeName, methodName);
                    if (!methodDirectEvents.ContainsKey(methodKey)) methodDirectEvents[methodKey] = new List<string>();
                    if (!methodCallGraph.ContainsKey(methodKey)) methodCallGraph[methodKey] = new List<(string, string)>();
                    if (!allAggregateMethods.ContainsKey(methodKey)) allAggregateMethods[methodKey] = displayMethodName;
                    foreach (var invocation in ctor.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        if (invocation.IsAddDomainEventInvocation(model, out var eventTypeSymbol) && eventTypeSymbol != null)
                        {
                            methodDirectEvents[methodKey].Add(eventTypeSymbol.ToDisplayString());
                        }
                        else if (invocation.Expression is MemberAccessExpressionSyntax callMemberAccess)
                        {
                            var calledSymbol = model.GetSymbolInfo(callMemberAccess).Symbol as IMethodSymbol;
                            if (calledSymbol != null)
                            {
                                var calleeType = calledSymbol.ContainingType.ToDisplayString();
                                var calleeKey = (calleeType, calledSymbol.MethodKind == MethodKind.Constructor ? ".ctor" : calledSymbol.Name);
                                methodCallGraph[methodKey].Add(calleeKey);
                            }
                        }
                    }
                }
            }

            // 调试信息收集
            var debugLines = new List<string>();
            // 步骤4：生成聚合方法与事件的映射关系
            var aggregates = new List<(string AggregateType, string MethodName, string[] EventTypes)>();
            foreach (var kvp in allAggregateMethods)
            {
                var aggType = kvp.Key.Item1;
                var methodName = kvp.Value; // 用 displayMethodName
                var allEvents = CollectAllEvents(kvp.Key);
                aggregates.Add((aggType, methodName, allEvents.ToArray()));
                // PayAndRename 调试信息
                if (methodName.Contains("PayAndRename"))
                {
                    // 输出 methodCallGraph
                    if (methodCallGraph.TryGetValue(kvp.Key, out var callees))
                    {
                        debugLines.Add($"[DEBUG] PayAndRename methodCallGraph: AggregateType={aggType}, MethodName={methodName}, Callees=[{string.Join(", ", callees.Select(c => $"{c.Item1}.{c.Item2}"))}]");
                    }
                    debugLines.Add($"[DEBUG] PayAndRename事件链: AggregateType={aggType}, MethodName={methodName}, Events=[{string.Join(", ", allEvents)}]");
                }
            }
            HashSet<string> CollectAllEvents((string, string) method, HashSet<(string, string)>? visited = null)
            {
                visited ??= new HashSet<(string, string)>();
                if (visited.Contains(method)) return new HashSet<string>();
                visited.Add(method);
                var events = new HashSet<string>();
                // 1. 当前方法的事件
                if (methodDirectEvents.TryGetValue(method, out var directEvents))
                    events.UnionWith(directEvents.Where(e => !string.IsNullOrEmpty(e))!);
                // 2. 递归所有被调用方法
                if (methodCallGraph.TryGetValue(method, out var callees))
                {
                    foreach (var callee in callees)
                        events.UnionWith(CollectAllEvents(callee, visited));
                }
                // 3. 合并所有同名方法的事件（聚合根和子实体同名方法）
                foreach (var kv in methodDirectEvents)
                {
                    if (kv.Key != method && kv.Key.Item2 == method.Item2)
                    {
                        events.UnionWith(kv.Value.Where(e => !string.IsNullOrEmpty(e))!);
                    }
                }
                return events;
            }
            // 去重输出
            var distinctAggregates = aggregates
                .GroupBy(a => (a.AggregateType, a.MethodName))
                .Select(g => g.First())
                .ToList();
            if (distinctAggregates.Count > 0)
            {
                var sb = new StringBuilder();
                // 输出调试信息为注释
                foreach (var line in debugLines)
                {
                    sb.AppendLine($"// {line}");
                }
                sb.AppendLine("// <auto-generated/>\nusing System;\nusing NetCorePal.Extensions.CodeAnalysis.Attributes;");
                foreach (var agg in distinctAggregates)
                {
                    sb.AppendLine($"// DEBUG: AggregateType: {agg.AggregateType}, MethodName: {agg.MethodName}, EventTypes.Length: {(agg.EventTypes == null ? 0 : agg.EventTypes.Length)}, EventTypes: [{string.Join(", ", agg.EventTypes ?? Array.Empty<string>())}]");
                    if (agg.EventTypes != null && agg.EventTypes.Length > 0)
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

            // 辅助方法
            bool IsChildEntityType(INamedTypeSymbol typeSymbol, INamedTypeSymbol aggregateSymbol)
            {
                return typeSymbol.AllInterfaces.Any(i => i.Name == "IEntity") && !typeSymbol.Equals(aggregateSymbol, SymbolEqualityComparer.Default);
            }
        });
    }
} 