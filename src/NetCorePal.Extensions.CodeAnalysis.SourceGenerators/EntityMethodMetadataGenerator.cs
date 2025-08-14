using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.SourceGenerators;

[Generator]
public class EntityMethodMetadataGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: (ctx, _) => ctx.Node as MethodDeclarationSyntax)
            .Where(m => m != null);

        var constructorDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ConstructorDeclarationSyntax,
                transform: (ctx, _) => ctx.Node as ConstructorDeclarationSyntax)
            .Where(c => c != null);

        var compilationAndMethods = context.CompilationProvider.Combine(methodDeclarations.Collect());
        var compilationAndConstructors = context.CompilationProvider.Combine(constructorDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndMethods, (spc, source) =>
        {
            ProcessMethods(spc, source.Item1, source.Item2);
        });

        context.RegisterSourceOutput(compilationAndConstructors, (spc, source) =>
        {
            ProcessConstructors(spc, source.Item1, source.Item2);
        });
    }

    private void ProcessMethods(SourceProductionContext spc, Compilation compilation, ImmutableArray<MethodDeclarationSyntax?> methodNodes)
    {
        var metas = new List<(string EntityType, string MethodName, List<string> EventTypes, List<string> CalledEntityMethods)>();

        foreach (var method in methodNodes)
        {
            if (method == null) continue;
            var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
            if (semanticModel == null) continue;
            var methodSymbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
            if (methodSymbol == null) continue;
            var containingType = methodSymbol.ContainingType;
            if (containingType == null || containingType.DeclaredAccessibility != Accessibility.Public) continue;
            
            // 处理所有方法（公共、私有、受保护、内部、静态、实例方法等）
            // 不再限制方法的访问性
            
            // 只为实体类型生成元数据
            if (!containingType.IsEntity()) continue;

            // 查找方法体内所有 new 表达式，判断是否为领域事件类型
            var eventTypes = new HashSet<string>();
            var calledEntityMethods = new HashSet<string>();
            if (method.Body != null)
            {
                foreach (var objCreation in method.Body.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    var typeInfo = semanticModel.GetTypeInfo(objCreation).Type;
                    if (typeInfo is INamedTypeSymbol namedType && namedType.IsDomainEvent())
                    {
                        eventTypes.Add(namedType.ToDisplayString());
                    }
                }

                // 查找方法体内所有方法调用，判断是否为其它实体方法
                foreach (var invocation in method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    var invokedSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (invokedSymbol == null) continue;
                    var targetType = invokedSymbol.ContainingType;
                    if (targetType != null && targetType.IsEntity())
                    {
                        calledEntityMethods.Add($"{targetType.ToDisplayString()}.{invokedSymbol.Name}");
                    }
                }
            }
            
            // 为所有实体方法生成元数据，即使没有发出事件或调用其他实体方法
            metas.Add((containingType.ToDisplayString(), methodSymbol.Name, eventTypes.ToList(), calledEntityMethods.ToList()));
        }

        GenerateMetadata(spc, metas, "EntityMethodMetadata.g.cs");
    }

    private void ProcessConstructors(SourceProductionContext spc, Compilation compilation, ImmutableArray<ConstructorDeclarationSyntax?> constructorNodes)
    {
        var metas = new List<(string EntityType, string MethodName, List<string> EventTypes, List<string> CalledEntityMethods)>();

        foreach (var constructor in constructorNodes)
        {
            if (constructor == null) continue;
            var semanticModel = compilation.GetSemanticModel(constructor.SyntaxTree);
            if (semanticModel == null) continue;
            var methodSymbol = semanticModel.GetDeclaredSymbol(constructor) as IMethodSymbol;
            if (methodSymbol == null) continue;
            var containingType = methodSymbol.ContainingType;
            if (containingType == null || containingType.DeclaredAccessibility != Accessibility.Public) continue;
            
            // 处理所有构造函数（公共、私有、受保护、内部等）
            // 不再限制构造函数的访问性
            
            // 只为实体类型生成元数据
            if (!containingType.IsEntity()) continue;

            // 查找构造函数体内所有 new 表达式，判断是否为领域事件类型
            var eventTypes = new HashSet<string>();
            var calledEntityMethods = new HashSet<string>();
            if (constructor.Body != null)
            {
                foreach (var objCreation in constructor.Body.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    var typeInfo = semanticModel.GetTypeInfo(objCreation).Type;
                    if (typeInfo is INamedTypeSymbol namedType && namedType.IsDomainEvent())
                    {
                        eventTypes.Add(namedType.ToDisplayString());
                    }
                }

                // 查找构造函数体内所有方法调用，判断是否为其它实体方法
                foreach (var invocation in constructor.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    var invokedSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (invokedSymbol == null) continue;
                    var targetType = invokedSymbol.ContainingType;
                    if (targetType != null && targetType.IsEntity())
                    {
                        calledEntityMethods.Add($"{targetType.ToDisplayString()}.{invokedSymbol.Name}");
                    }
                }
            }
            
            // 为所有实体构造函数生成元数据
            metas.Add((containingType.ToDisplayString(), ".ctor", eventTypes.ToList(), calledEntityMethods.ToList()));
        }

        GenerateMetadata(spc, metas, "EntityConstructorMetadata.g.cs");
    }

    private void GenerateMetadata(SourceProductionContext spc, List<(string EntityType, string MethodName, List<string> EventTypes, List<string> CalledEntityMethods)> metas, string fileName)
    {
        if (metas.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>\nusing System;\nusing NetCorePal.Extensions.CodeAnalysis.Attributes;");
            foreach (var (entityType, methodName, eventTypes, calledEntityMethods) in metas)
            {
                var eventsLiteral = string.Join(", ", eventTypes.Select(e => $"\"{e}\""));
                var calledMethodsLiteral = string.Join(", ", calledEntityMethods.Select(m => $"\"{m}\""));
                sb.AppendLine($"[assembly: EntityMethodMetadataAttribute(\"{entityType}\", \"{methodName}\", new string[] {{ {eventsLiteral} }}, new string[] {{ {calledMethodsLiteral} }})]\n");
            }
            spc.AddSource(fileName, sb.ToString());
        }
    }
}
