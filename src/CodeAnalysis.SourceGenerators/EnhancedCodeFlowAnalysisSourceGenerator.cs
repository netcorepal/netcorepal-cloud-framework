using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis.SourceGenerators
{
    /// <summary>
    /// 增强的代码流分析源生成器 - 包含完整的调用链分析和可视化支持
    /// </summary>
    [Generator]
    public class EnhancedCodeFlowAnalysisSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 配置选项管道
            var configOptions = context.AnalyzerConfigOptionsProvider
                .Select(static (options, _) => 
                    options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var ns) 
                        ? ns 
                        : "Generated");

            // 语法分析管道
            var syntaxProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax or MethodDeclarationSyntax,
                    transform: (syntaxContext, _) => syntaxContext)
                .Where(ctx => ctx.Node != null);

            var compilationAndSyntax = context.CompilationProvider
                .Combine(syntaxProvider.Collect())
                .Combine(configOptions);

            context.RegisterSourceOutput(compilationAndSyntax, (spc, source) =>
            {
                var ((compilation, syntaxContexts), rootNamespace) = source;
                var analyzer = new EnhancedCodeFlowAnalyzer(compilation, rootNamespace ?? "Generated");
                
                // 添加诊断信息
                var diagnosticInfo = $@"
// EnhancedCodeFlowAnalysisSourceGenerator 已执行
// 找到的语法节点数量: {syntaxContexts.Length}
// 编译名称: {compilation.AssemblyName}
// 根命名空间: {rootNamespace ?? "Generated"}
";
                
                // 分析所有语法节点
                foreach (var syntaxContext in syntaxContexts)
                {
                    analyzer.AnalyzeSyntaxNode(syntaxContext);
                }

                // 生成分析结果
                GenerateAnalysisResults(spc, analyzer, diagnosticInfo);
            });
        }

        private void GenerateAnalysisResults(SourceProductionContext context, EnhancedCodeFlowAnalyzer analyzer, string diagnosticInfo = "")
        {
            // 生成主要的分析结果类
            var mainResult = analyzer.GenerateMainAnalysisResult(diagnosticInfo);
            context.AddSource("CodeFlowAnalysis.g.cs", SourceText.From(mainResult, Encoding.UTF8));

            // 生成JSON格式的分析结果（用于可视化）
            var jsonResult = analyzer.GenerateJsonAnalysisResult();
            context.AddSource("CodeFlowAnalysis.json.g.cs", SourceText.From(jsonResult, Encoding.UTF8));

            // 生成Mermaid图表代码
            var mermaidResult = analyzer.GenerateMermaidDiagram();
            context.AddSource("CodeFlowMermaid.g.cs", SourceText.From(mermaidResult, Encoding.UTF8));

            // 生成统计信息
            var statsResult = analyzer.GenerateStatistics();
            context.AddSource("CodeFlowStatistics.g.cs", SourceText.From(statsResult, Encoding.UTF8));
        }
    }

    /// <summary>
    /// 增强的代码流分析器
    /// </summary>
    public class EnhancedCodeFlowAnalyzer
    {
        private readonly Compilation _compilation;
        private readonly string _rootNamespace;
        private readonly List<ControllerAnalysis> _controllers = new();
        private readonly List<CommandAnalysis> _commands = new();
        private readonly List<CommandHandlerAnalysis> _commandHandlers = new();
        private readonly List<EntityAnalysis> _entities = new();
        private readonly List<DomainEventAnalysis> _domainEvents = new();
        private readonly List<DomainEventHandlerAnalysis> _domainEventHandlers = new();
        private readonly List<CallRelationshipAnalysis> _relationships = new();

        public EnhancedCodeFlowAnalyzer(Compilation compilation, string rootNamespace)
        {
            _compilation = compilation;
            _rootNamespace = rootNamespace;
        }

        public void AnalyzeSyntaxNode(GeneratorSyntaxContext syntaxContext)
        {
            var semanticModel = syntaxContext.SemanticModel;
            var node = syntaxContext.Node;

            switch (node)
            {
                case ClassDeclarationSyntax classDeclaration:
                    AnalyzeClassDeclaration(classDeclaration, semanticModel);
                    break;
                case RecordDeclarationSyntax recordDeclaration:
                    AnalyzeRecordDeclaration(recordDeclaration, semanticModel);
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    AnalyzeMethodDeclaration(methodDeclaration, semanticModel);
                    break;
            }
        }

        private void AnalyzeClassDeclaration(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            if (symbol == null) return;

            var analysis = new TypeAnalysis(symbol, classDeclaration);

            // 识别不同类型的类
            if (IsController(symbol))
            {
                _controllers.Add(new ControllerAnalysis(analysis, GetControllerActions(classDeclaration, semanticModel)));
            }
            else if (IsCommand(symbol))
            {
                _commands.Add(new CommandAnalysis(analysis, GetCommandParameters(classDeclaration, semanticModel)));
            }
            else if (IsCommandHandler(symbol))
            {
                var handledCommandType = GetHandledCommandType(symbol);
                _commandHandlers.Add(new CommandHandlerAnalysis(analysis, handledCommandType, GetCommandHandlerCalls(classDeclaration, semanticModel)));
            }
            else if (IsEntity(symbol))
            {
                var isAggregateRoot = IsAggregateRoot(symbol);
                var methods = GetEntityMethods(classDeclaration, semanticModel);
                var domainEvents = GetEntityDomainEvents(classDeclaration, semanticModel);
                _entities.Add(new EntityAnalysis(analysis, isAggregateRoot, methods, domainEvents));
            }
            else if (IsDomainEventHandler(symbol))
            {
                var handledEventType = GetHandledDomainEventType(symbol);
                var commands = GetDomainEventHandlerCommands(classDeclaration, semanticModel);
                _domainEventHandlers.Add(new DomainEventHandlerAnalysis(analysis, handledEventType, commands));
            }
        }

        private void AnalyzeRecordDeclaration(RecordDeclarationSyntax recordDeclaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(recordDeclaration);
            if (symbol == null) return;

            var analysis = new TypeAnalysis(symbol, recordDeclaration);

            // 识别领域事件（通常是record类型）
            if (IsDomainEvent(symbol))
            {
                var properties = GetDomainEventProperties(recordDeclaration, semanticModel);
                var entityType = GetDomainEventEntityType(recordDeclaration, semanticModel);
                _domainEvents.Add(new DomainEventAnalysis(analysis, properties, entityType));
            }
            else if (IsCommand(symbol))
            {
                var parameters = GetCommandParameters(recordDeclaration, semanticModel);
                _commands.Add(new CommandAnalysis(analysis, parameters));
            }
        }

        private void AnalyzeMethodDeclaration(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            // 分析方法调用关系
            AnalyzeMethodCalls(methodDeclaration, semanticModel);
        }

        private void AnalyzeMethodCalls(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null) return;

            var containingType = methodSymbol.ContainingType;
            var sourceInfo = new MethodInfo(containingType.ToDisplayString(), methodSymbol.Name);

            // 分析各种调用类型
            AnalyzeMediatorCalls(methodDeclaration, semanticModel, sourceInfo);
            AnalyzeDomainEventCalls(methodDeclaration, semanticModel, sourceInfo);
            AnalyzeEntityMethodCalls(methodDeclaration, semanticModel, sourceInfo);
        }

        private void AnalyzeMediatorCalls(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, MethodInfo sourceInfo)
        {
            var invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            
            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol calledMethod)
                {
                    // 检查是否是mediator.Send调用
                    if (calledMethod.Name == "Send" && 
                        calledMethod.ContainingType.AllInterfaces.Any(i => i.Name == "IMediator"))
                    {
                        var commandType = GetCommandTypeFromArgument(invocation, semanticModel);
                        if (!string.IsNullOrEmpty(commandType))
                        {
                            var callType = DetermineCallTypeFromSource(sourceInfo.TypeName);
                            _relationships.Add(new CallRelationshipAnalysis
                            {
                                Source = sourceInfo,
                                Target = new MethodInfo(commandType, "Handle"),
                                CallType = callType,
                                CallLocation = GetLocationInfo(invocation)
                            });
                        }
                    }
                }
            }
        }

        private void AnalyzeDomainEventCalls(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, MethodInfo sourceInfo)
        {
            var invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            
            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol calledMethod)
                {
                    // 检查是否是AddDomainEvent调用
                    if (calledMethod.Name == "AddDomainEvent")
                    {
                        var domainEventType = GetDomainEventTypeFromArgument(invocation, semanticModel);
                        if (!string.IsNullOrEmpty(domainEventType))
                        {
                            _relationships.Add(new CallRelationshipAnalysis
                            {
                                Source = sourceInfo,
                                Target = new MethodInfo(domainEventType, "Create"),
                                CallType = CallType.EntityToDomainEvent,
                                CallLocation = GetLocationInfo(invocation)
                            });
                        }
                    }
                }
            }
        }

        private void AnalyzeEntityMethodCalls(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, MethodInfo sourceInfo)
        {
            var invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            
            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol calledMethod)
                {
                    var targetType = calledMethod.ContainingType;
                    
                    // 检查是否调用了实体方法
                    if (IsEntity(targetType))
                    {
                        _relationships.Add(new CallRelationshipAnalysis
                        {
                            Source = sourceInfo,
                            Target = new MethodInfo(targetType.ToDisplayString(), calledMethod.Name),
                            CallType = CallType.CommandToEntity,
                            CallLocation = GetLocationInfo(invocation)
                        });
                    }
                }
            }
        }

        #region 类型识别方法

        private bool IsController(INamedTypeSymbol symbol)
        {
            return symbol.Name.EndsWith("Controller") ||
                   symbol.AllInterfaces.Any(i => i.Name == "IController") ||
                   HasBaseType(symbol, "ControllerBase", "Controller");
        }

        private bool IsCommand(INamedTypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "ICommand" || i.Name == "IBaseCommand") ||
                   symbol.Name.EndsWith("Command");
        }

        private bool IsCommandHandler(INamedTypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => 
                i.IsGenericType && i.ConstructedFrom.Name == "ICommandHandler") ||
                   symbol.Name.EndsWith("CommandHandler");
        }

        private bool IsEntity(INamedTypeSymbol symbol)
        {
            return HasBaseType(symbol, "Entity") ||
                   (symbol.BaseType?.IsGenericType == true && 
                    symbol.BaseType.ConstructedFrom.Name == "Entity");
        }

        private bool IsAggregateRoot(INamedTypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "IAggregateRoot");
        }

        private bool IsDomainEvent(INamedTypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "IDomainEvent") ||
                   symbol.Name.EndsWith("DomainEvent");
        }

        private bool IsDomainEventHandler(INamedTypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => 
                i.IsGenericType && i.ConstructedFrom.Name == "IDomainEventHandler") ||
                   symbol.Name.EndsWith("DomainEventHandler");
        }

        private bool HasBaseType(INamedTypeSymbol symbol, params string[] baseTypeNames)
        {
            var current = symbol.BaseType;
            while (current != null)
            {
                if (baseTypeNames.Contains(current.Name) || 
                    (current.IsGenericType && baseTypeNames.Contains(current.ConstructedFrom.Name)))
                {
                    return true;
                }
                current = current.BaseType;
            }
            return false;
        }

        #endregion

        #region 类型信息提取方法

        private string GetHandledCommandType(INamedTypeSymbol symbol)
        {
            var commandHandlerInterface = symbol.AllInterfaces
                .FirstOrDefault(i => i.IsGenericType && i.ConstructedFrom.Name == "ICommandHandler");
            
            return commandHandlerInterface?.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "";
        }

        private string GetHandledDomainEventType(INamedTypeSymbol symbol)
        {
            var domainEventHandlerInterface = symbol.AllInterfaces
                .FirstOrDefault(i => i.IsGenericType && i.ConstructedFrom.Name == "IDomainEventHandler");
            
            return domainEventHandlerInterface?.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "";
        }

        private string GetCommandTypeFromArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count > 0)
            {
                var argument = arguments[0];
                var typeInfo = semanticModel.GetTypeInfo(argument.Expression);
                return typeInfo.Type?.ToDisplayString() ?? "";
            }
            return "";
        }

        private string GetDomainEventTypeFromArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count > 0)
            {
                var argument = arguments[0];
                if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                {
                    var typeInfo = semanticModel.GetTypeInfo(objectCreation);
                    return typeInfo.Type?.ToDisplayString() ?? "";
                }
            }
            return "";
        }

        private LocationInfo GetLocationInfo(SyntaxNode node)
        {
            var location = node.GetLocation();
            var lineSpan = location.GetLineSpan();
            return new LocationInfo
            {
                FileName = location.SourceTree?.FilePath ?? "",
                Line = lineSpan.StartLinePosition.Line + 1,
                Column = lineSpan.StartLinePosition.Character + 1
            };
        }

        private CallType DetermineCallTypeFromSource(string sourceTypeName)
        {
            if (sourceTypeName.EndsWith("Controller"))
                return CallType.ControllerToCommand;
            if (sourceTypeName.EndsWith("DomainEventHandler"))
                return CallType.DomainEventHandlerToCommand;
            if (sourceTypeName.EndsWith("CommandHandler"))
                return CallType.CommandToEntity;
            
            return CallType.Other;
        }

        #endregion

        #region 数据提取方法

        private List<ActionInfo> GetControllerActions(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var actions = new List<ActionInfo>();
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            
            foreach (var method in methods)
            {
                var httpAttributes = method.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Where(a => IsHttpAttribute(a.Name.ToString()))
                    .ToList();

                if (httpAttributes.Any())
                {
                    var httpMethod = GetHttpMethodFromAttribute(httpAttributes.First());
                    var routeTemplate = GetRouteFromAttributes(method);
                    var commands = GetCommandsCalledInMethod(method, semanticModel);
                    
                    actions.Add(new ActionInfo
                    {
                        Name = method.Identifier.ValueText,
                        HttpMethod = httpMethod,
                        RouteTemplate = routeTemplate,
                        Commands = commands
                    });
                }
            }
            
            return actions;
        }

        private List<ParameterInfo> GetCommandParameters(SyntaxNode declaration, SemanticModel semanticModel)
        {
            var parameters = new List<ParameterInfo>();
            
            IEnumerable<PropertyDeclarationSyntax> properties = declaration switch
            {
                ClassDeclarationSyntax classDecl => classDecl.Members.OfType<PropertyDeclarationSyntax>(),
                RecordDeclarationSyntax recordDecl => recordDecl.Members.OfType<PropertyDeclarationSyntax>(),
                _ => Enumerable.Empty<PropertyDeclarationSyntax>()
            };

            foreach (var property in properties)
            {
                var typeInfo = semanticModel.GetTypeInfo(property.Type);
                parameters.Add(new ParameterInfo
                {
                    Name = property.Identifier.ValueText,
                    Type = typeInfo.Type?.ToDisplayString() ?? property.Type.ToString(),
                    IsRequired = !property.Type.ToString().EndsWith("?")
                });
            }
            
            return parameters;
        }

        private List<EntityMethodInfo> GetEntityMethods(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var methods = new List<EntityMethodInfo>();
            var methodDeclarations = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)));

            foreach (var method in methodDeclarations)
            {
                var domainEvents = GetDomainEventsFromMethod(method, semanticModel);
                methods.Add(new EntityMethodInfo
                {
                    Name = method.Identifier.ValueText,
                    DomainEvents = domainEvents
                });
            }
            
            return methods;
        }

        private List<string> GetEntityDomainEvents(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var domainEvents = new List<string>();
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            
            foreach (var method in methods)
            {
                domainEvents.AddRange(GetDomainEventsFromMethod(method, semanticModel));
            }
            
            return domainEvents.Distinct().ToList();
        }

        private List<string> GetDomainEventsFromMethod(MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            var domainEvents = new List<string>();
            var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
            
            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol &&
                    methodSymbol.Name == "AddDomainEvent")
                {
                    var eventType = GetDomainEventTypeFromArgument(invocation, semanticModel);
                    if (!string.IsNullOrEmpty(eventType))
                    {
                        domainEvents.Add(eventType);
                    }
                }
            }
            
            return domainEvents;
        }

        private List<string> GetCommandsCalledInMethod(MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            var commands = new List<string>();
            var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
            
            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol &&
                    methodSymbol.Name == "Send" &&
                    methodSymbol.ContainingType.AllInterfaces.Any(i => i.Name == "IMediator"))
                {
                    var commandType = GetCommandTypeFromArgument(invocation, semanticModel);
                    if (!string.IsNullOrEmpty(commandType))
                    {
                        commands.Add(commandType);
                    }
                }
            }
            
            return commands;
        }

        private List<ParameterInfo> GetDomainEventProperties(RecordDeclarationSyntax recordDeclaration, SemanticModel semanticModel)
        {
            var properties = new List<ParameterInfo>();
            
            // 分析record的主构造函数参数
            if (recordDeclaration.ParameterList != null)
            {
                foreach (var parameter in recordDeclaration.ParameterList.Parameters)
                {
                    var typeInfo = semanticModel.GetTypeInfo(parameter.Type!);
                    properties.Add(new ParameterInfo
                    {
                        Name = parameter.Identifier.ValueText,
                        Type = typeInfo.Type?.ToDisplayString() ?? parameter.Type!.ToString(),
                        IsRequired = true
                    });
                }
            }
            
            return properties;
        }

        private string GetDomainEventEntityType(RecordDeclarationSyntax recordDeclaration, SemanticModel semanticModel)
        {
            // 尝试从参数中识别实体类型
            if (recordDeclaration.ParameterList != null)
            {
                foreach (var parameter in recordDeclaration.ParameterList.Parameters)
                {
                    var typeInfo = semanticModel.GetTypeInfo(parameter.Type!);
                    if (typeInfo.Type is INamedTypeSymbol namedType && IsEntity(namedType))
                    {
                        return typeInfo.Type.ToDisplayString();
                    }
                }
            }
            
            return "";
        }

        private List<string> GetDomainEventHandlerCommands(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var commands = new List<string>();
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            
            foreach (var method in methods)
            {
                commands.AddRange(GetCommandsCalledInMethod(method, semanticModel));
            }
            
            return commands.Distinct().ToList();
        }

        private List<string> GetCommandHandlerCalls(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var calls = new List<string>();
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            
            foreach (var method in methods)
            {
                var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
                foreach (var invocation in invocations)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                    if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                    {
                        var targetType = methodSymbol.ContainingType;
                        if (IsEntity(targetType))
                        {
                            calls.Add($"{targetType.ToDisplayString()}.{methodSymbol.Name}");
                        }
                    }
                }
            }
            
            return calls.Distinct().ToList();
        }

        #endregion

        #region 工具方法

        private bool IsHttpAttribute(string attributeName)
        {
            var httpAttributes = new[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch", "HttpHead", "HttpOptions" };
            return httpAttributes.Any(attr => attributeName.Contains(attr));
        }

        private string GetHttpMethodFromAttribute(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            if (name.Contains("Get")) return "GET";
            if (name.Contains("Post")) return "POST";
            if (name.Contains("Put")) return "PUT";
            if (name.Contains("Delete")) return "DELETE";
            if (name.Contains("Patch")) return "PATCH";
            if (name.Contains("Head")) return "HEAD";
            if (name.Contains("Options")) return "OPTIONS";
            return "GET";
        }

        private string GetRouteFromAttributes(MethodDeclarationSyntax method)
        {
            var routeAttributes = method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Where(a => a.Name.ToString().Contains("Route"));

            foreach (var attr in routeAttributes)
            {
                if (attr.ArgumentList?.Arguments.Count > 0)
                {
                    var argument = attr.ArgumentList.Arguments[0];
                    if (argument.Expression is LiteralExpressionSyntax literal)
                    {
                        return literal.Token.ValueText;
                    }
                }
            }

            return "";
        }

        #endregion

        #region 结果生成方法

        public string GenerateMainAnalysisResult(string diagnosticInfo = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// Enhanced Code Flow Analysis Result");
            sb.AppendLine(diagnosticInfo);
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_rootNamespace}.CodeAnalysis");
            sb.AppendLine("{");
            
            // 添加兼容性别名（必须在命名空间内，类声明之前）
            sb.AppendLine("    // 兼容性别名");
            sb.AppendLine("    using LocationInfo = EnhancedLocationInfo;");
            sb.AppendLine("    using TypeAnalysis = EnhancedTypeAnalysis;");
            sb.AppendLine("    using ControllerAnalysis = EnhancedControllerAnalysis;");
            sb.AppendLine("    using ActionInfo = EnhancedActionInfo;");
            sb.AppendLine("    using CommandAnalysis = EnhancedCommandAnalysis;");
            sb.AppendLine("    using ParameterInfo = EnhancedParameterInfo;");
            sb.AppendLine("    using CommandHandlerAnalysis = EnhancedCommandHandlerAnalysis;");
            sb.AppendLine("    using MethodCallInfo = EnhancedMethodCallInfo;");
            sb.AppendLine("    using EntityAnalysis = EnhancedEntityAnalysis;");
            sb.AppendLine("    using EntityMethodInfo = EnhancedEntityMethodInfo;");
            sb.AppendLine("    using DomainEventAnalysis = EnhancedDomainEventAnalysis;");
            sb.AppendLine("    using PropertyInfo = EnhancedPropertyInfo;");
            sb.AppendLine("    using DomainEventHandlerAnalysis = EnhancedDomainEventHandlerAnalysis;");
            sb.AppendLine("    using CallRelationshipAnalysis = EnhancedCallRelationshipAnalysis;");
            sb.AppendLine("    using CallChain = EnhancedCallChain;");
            sb.AppendLine();
            
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Enhanced Code Flow Analysis Result");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class EnhancedCodeFlowAnalysis");
            sb.AppendLine("    {");

            // 生成数据集合
            GenerateDataCollections(sb);
            
            // 生成查询方法
            GenerateQueryMethods(sb);

            sb.AppendLine("    }");
            
            // 生成数据模型类
            GenerateDataModels(sb);
            
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void GenerateDataCollections(StringBuilder sb)
        {
            // Controllers
            sb.AppendLine("        public static readonly List<EnhancedControllerAnalysis> Controllers = new()");
            sb.AppendLine("        {");
            foreach (var controller in _controllers)
            {
                sb.AppendLine($"            // {controller.Type.Name}");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // Commands
            sb.AppendLine("        public static readonly List<EnhancedCommandAnalysis> Commands = new()");
            sb.AppendLine("        {");
            foreach (var command in _commands)
            {
                sb.AppendLine($"            // {command.Type.Name}");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // Command Handlers
            sb.AppendLine("        public static readonly List<EnhancedCommandHandlerAnalysis> CommandHandlers = new()");
            sb.AppendLine("        {");
            foreach (var handler in _commandHandlers)
            {
                sb.AppendLine($"            // {handler.Type.Name}");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // Entities
            sb.AppendLine("        public static readonly List<EnhancedEntityAnalysis> Entities = new()");
            sb.AppendLine("        {");
            foreach (var entity in _entities)
            {
                sb.AppendLine($"            // {entity.Type.Name}");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // Domain Events
            sb.AppendLine("        public static readonly List<EnhancedDomainEventAnalysis> DomainEvents = new()");
            sb.AppendLine("        {");
            foreach (var domainEvent in _domainEvents)
            {
                sb.AppendLine($"            // {domainEvent.Type.Name}");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // Domain Event Handlers
            sb.AppendLine("        public static readonly List<EnhancedDomainEventHandlerAnalysis> DomainEventHandlers = new()");
            sb.AppendLine("        {");
            foreach (var handler in _domainEventHandlers)
            {
                sb.AppendLine($"            // {handler.Type.Name}");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // Call Relationships
            sb.AppendLine("        public static readonly List<EnhancedCallRelationshipAnalysis> Relationships = new()");
            sb.AppendLine("        {");
            foreach (var relationship in _relationships)
            {
                sb.AppendLine($"            // {relationship.Source.TypeName} -> {relationship.Target.TypeName}");
            }
            sb.AppendLine("        };");
            sb.AppendLine();
        }

        private void GenerateQueryMethods(StringBuilder sb)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取控制器调用的所有命令");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static List<string> GetCommandsCalledByController(string controllerName)");
            sb.AppendLine("        {");
            sb.AppendLine("            var controller = Controllers.FirstOrDefault(c => c.Type.Name == controllerName);");
            sb.AppendLine("            return controller?.Actions.SelectMany(a => a.Commands).Distinct().ToList() ?? new List<string>();");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取实体方法创建的领域事件");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static List<string> GetDomainEventsCreatedByEntityMethod(string entityName, string methodName)");
            sb.AppendLine("        {");
            sb.AppendLine("            var entity = Entities.FirstOrDefault(e => e.Type.Name == entityName);");
            sb.AppendLine("            var method = entity?.Methods.FirstOrDefault(m => m.Name == methodName);");
            sb.AppendLine("            return method?.DomainEvents ?? new List<string>();");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取领域事件处理器发出的命令");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static List<string> GetCommandsIssuedByDomainEventHandler(string handlerName)");
            sb.AppendLine("        {");
            sb.AppendLine("            var handler = DomainEventHandlers.FirstOrDefault(h => h.Type.Name == handlerName);");
            sb.AppendLine("            return handler?.Commands ?? new List<string>();");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取完整的调用链路：Controller -> Command -> Entity -> DomainEvent -> DomainEventHandler");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static List<EnhancedCallChain> GetCompleteCallChains()");
            sb.AppendLine("        {");
            sb.AppendLine("            var chains = new List<EnhancedCallChain>();");
            sb.AppendLine("            // Implementation would go here");
            sb.AppendLine("            return chains;");
            sb.AppendLine("        }");
        }

        private void GenerateDataModels(StringBuilder sb)
        {
            // 这里生成所有的数据模型类
            sb.AppendLine();
            sb.AppendLine("    #region Enhanced Data Models");
            sb.AppendLine();
            
            // LocationInfo
            sb.AppendLine("    public class EnhancedLocationInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string FilePath { get; set; } = \"\";");
            sb.AppendLine("        public int StartLine { get; set; }");
            sb.AppendLine("        public int EndLine { get; set; }");
            sb.AppendLine("        public int StartColumn { get; set; }");
            sb.AppendLine("        public int EndColumn { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();

            // TypeAnalysis
            sb.AppendLine("    public class EnhancedTypeAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string FullName { get; set; } = \"\";");
            sb.AppendLine("        public string Namespace { get; set; } = \"\";");
            sb.AppendLine("        public EnhancedLocationInfo Location { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // ControllerAnalysis
            sb.AppendLine("    public class EnhancedControllerAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedTypeAnalysis Type { get; set; } = new();");
            sb.AppendLine("        public List<EnhancedActionInfo> Actions { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // ActionInfo
            sb.AppendLine("    public class EnhancedActionInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string HttpMethod { get; set; } = \"\";");
            sb.AppendLine("        public string RouteTemplate { get; set; } = \"\";");
            sb.AppendLine("        public List<string> Commands { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // CommandAnalysis
            sb.AppendLine("    public class EnhancedCommandAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedTypeAnalysis Type { get; set; } = new();");
            sb.AppendLine("        public List<EnhancedParameterInfo> Parameters { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // ParameterInfo
            sb.AppendLine("    public class EnhancedParameterInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string TypeName { get; set; } = \"\";");
            sb.AppendLine("        public bool IsRequired { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();

            // CommandHandlerAnalysis
            sb.AppendLine("    public class EnhancedCommandHandlerAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedTypeAnalysis Type { get; set; } = new();");
            sb.AppendLine("        public string HandledCommandType { get; set; } = \"\";");
            sb.AppendLine("        public List<EnhancedMethodCallInfo> Calls { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // MethodCallInfo
            sb.AppendLine("    public class EnhancedMethodCallInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string MethodName { get; set; } = \"\";");
            sb.AppendLine("        public string TargetType { get; set; } = \"\";");
            sb.AppendLine("        public EnhancedLocationInfo Location { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // MethodInfo
            sb.AppendLine("    public class EnhancedMethodInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string TypeName { get; set; } = \"\";");
            sb.AppendLine("        public string MethodName { get; set; } = \"\";");
            sb.AppendLine("    }");
            sb.AppendLine();

            // EntityAnalysis
            sb.AppendLine("    public class EnhancedEntityAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedTypeAnalysis Type { get; set; } = new();");
            sb.AppendLine("        public bool IsAggregateRoot { get; set; }");
            sb.AppendLine("        public List<EnhancedEntityMethodInfo> Methods { get; set; } = new();");
            sb.AppendLine("        public List<string> DomainEvents { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // EntityMethodInfo
            sb.AppendLine("    public class EnhancedEntityMethodInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public List<string> DomainEvents { get; set; } = new();");
            sb.AppendLine("        public EnhancedLocationInfo Location { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // DomainEventAnalysis
            sb.AppendLine("    public class EnhancedDomainEventAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedTypeAnalysis Type { get; set; } = new();");
            sb.AppendLine("        public List<EnhancedPropertyInfo> Properties { get; set; } = new();");
            sb.AppendLine("        public string EntityType { get; set; } = \"\";");
            sb.AppendLine("    }");
            sb.AppendLine();

            // PropertyInfo
            sb.AppendLine("    public class EnhancedPropertyInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string TypeName { get; set; } = \"\";");
            sb.AppendLine("        public bool IsReadOnly { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();

            // DomainEventHandlerAnalysis
            sb.AppendLine("    public class EnhancedDomainEventHandlerAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedTypeAnalysis Type { get; set; } = new();");
            sb.AppendLine("        public string HandledEventType { get; set; } = \"\";");
            sb.AppendLine("        public List<string> Commands { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // CallRelationshipAnalysis
            sb.AppendLine("    public class EnhancedCallRelationshipAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedMethodInfo Source { get; set; } = new();");
            sb.AppendLine("        public EnhancedMethodInfo Target { get; set; } = new();");
            sb.AppendLine("        public string CallType { get; set; } = \"\";");
            sb.AppendLine("        public EnhancedLocationInfo CallLocation { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // CallChain
            sb.AppendLine("    public class EnhancedCallChain");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Controller { get; set; } = \"\";");
            sb.AppendLine("        public string Action { get; set; } = \"\";");
            sb.AppendLine("        public string Command { get; set; } = \"\";");
            sb.AppendLine("        public string CommandHandler { get; set; } = \"\";");
            sb.AppendLine("        public List<string> EntitiesInvolved { get; set; } = new();");
            sb.AppendLine("        public List<string> DomainEventsRaised { get; set; } = new();");
            sb.AppendLine("        public List<string> DomainEventHandlers { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            sb.AppendLine("    #endregion");
        }

        public string GenerateJsonAnalysisResult()
        {
            var data = new
            {
                Controllers = _controllers,
                Commands = _commands,
                CommandHandlers = _commandHandlers,
                Entities = _entities,
                DomainEvents = _domainEvents,
                DomainEventHandlers = _domainEventHandlers,
                Relationships = _relationships
            };

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_rootNamespace}.CodeAnalysis.Enhanced");
            sb.AppendLine("{");
            sb.AppendLine("    public static class EnhancedCodeFlowAnalysisData");
            sb.AppendLine("{");
            sb.AppendLine("        // Enhanced analysis data structure generated by source generator");
            sb.AppendLine("        // TODO: Add enhanced data serialization when System.Text.Json is available");
            sb.AppendLine("    }");
            sb.AppendLine();

            // 添加数据模型类定义
            sb.AppendLine("    // Enhanced 数据模型类");
            sb.AppendLine("    public class EnhancedCommandAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public EnhancedTypeAnalysis Type { get; set; } = new();");
            sb.AppendLine("        public List<EnhancedParameterInfo> Parameters { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class EnhancedTypeAnalysis");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string FullName { get; set; } = \"\";");
            sb.AppendLine("        public string Namespace { get; set; } = \"\";");
            sb.AppendLine("        public EnhancedLocationInfo Location { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class EnhancedLocationInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string FileName { get; set; } = \"\";");
            sb.AppendLine("        public int LineNumber { get; set; }");
            sb.AppendLine("        public int ColumnNumber { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class EnhancedParameterInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string Type { get; set; } = \"\";");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class EnhancedCallChain");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Source { get; set; } = \"\";");
            sb.AppendLine("        public string Target { get; set; } = \"\";");
            sb.AppendLine("        public EnhancedCallType Type { get; set; }");
            sb.AppendLine("        public EnhancedLocationInfo Location { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public enum EnhancedCallType");
            sb.AppendLine("    {");
            sb.AppendLine("        ControllerToCommand,");
            sb.AppendLine("        CommandToEntity,");
            sb.AppendLine("        EntityToDomainEvent,");
            sb.AppendLine("        DomainEventHandlerToCommand,");
            sb.AppendLine("        Unknown");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }

        public string GenerateMermaidDiagram()
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine($"namespace {_rootNamespace}.CodeAnalysis");
            sb.AppendLine("{");
            sb.AppendLine("    public static class CodeFlowMermaidDiagram");
            sb.AppendLine("    {");
            sb.AppendLine("        public static readonly string FlowChart = @\"");
            
            // 生成 Mermaid 流程图
            sb.AppendLine("graph TD");
            
            // Controllers
            foreach (var controller in _controllers)
            {
                sb.AppendLine($"    {controller.Type.Name}[{controller.Type.Name}]");
                sb.AppendLine($"    {controller.Type.Name} --> Controller{{Controller}}");
            }

            // Commands
            foreach (var command in _commands)
            {
                sb.AppendLine($"    {command.Type.Name}[{command.Type.Name}]");
                sb.AppendLine($"    {command.Type.Name} --> Command{{Command}}");
            }

            // Entities
            foreach (var entity in _entities)
            {
                sb.AppendLine($"    {entity.Type.Name}[{entity.Type.Name}]");
                if (entity.IsAggregateRoot)
                {
                    sb.AppendLine($"    {entity.Type.Name} --> AggregateRoot{{Aggregate Root}}");
                }
                else
                {
                    sb.AppendLine($"    {entity.Type.Name} --> Entity{{Entity}}");
                }
            }

            // Domain Events
            foreach (var domainEvent in _domainEvents)
            {
                sb.AppendLine($"    {domainEvent.Type.Name}[{domainEvent.Type.Name}]");
                sb.AppendLine($"    {domainEvent.Type.Name} --> DomainEvent{{Domain Event}}");
            }

            // Relationships
            foreach (var relationship in _relationships)
            {
                var sourceTypeName = GetSimpleTypeName(relationship.Source.TypeName);
                var targetTypeName = GetSimpleTypeName(relationship.Target.TypeName);
                sb.AppendLine($"    {sourceTypeName} --> {targetTypeName}");
            }

            sb.AppendLine("        \";");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string GenerateStatistics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine($"namespace {_rootNamespace}.CodeAnalysis");
            sb.AppendLine("{");
            sb.AppendLine("    public static class CodeFlowStatistics");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static readonly int ControllerCount = {_controllers.Count};");
            sb.AppendLine($"        public static readonly int CommandCount = {_commands.Count};");
            sb.AppendLine($"        public static readonly int CommandHandlerCount = {_commandHandlers.Count};");
            sb.AppendLine($"        public static readonly int EntityCount = {_entities.Count};");
            sb.AppendLine($"        public static readonly int AggregateRootCount = {_entities.Count(e => e.IsAggregateRoot)};");
            sb.AppendLine($"        public static readonly int DomainEventCount = {_domainEvents.Count};");
            sb.AppendLine($"        public static readonly int DomainEventHandlerCount = {_domainEventHandlers.Count};");
            sb.AppendLine($"        public static readonly int RelationshipCount = {_relationships.Count};");
            sb.AppendLine();
            
            sb.AppendLine("        public static string GetSummary()");
            sb.AppendLine("        {");
            sb.AppendLine("            return $\"Code Flow Analysis Summary:\\n\" +");
            sb.AppendLine("                   $\"Controllers: {ControllerCount}\\n\" +");
            sb.AppendLine("                   $\"Commands: {CommandCount}\\n\" +");
            sb.AppendLine("                   $\"Command Handlers: {CommandHandlerCount}\\n\" +");
            sb.AppendLine("                   $\"Entities: {EntityCount}\\n\" +");
            sb.AppendLine("                   $\"Aggregate Roots: {AggregateRootCount}\\n\" +");
            sb.AppendLine("                   $\"Domain Events: {DomainEventCount}\\n\" +");
            sb.AppendLine("                   $\"Domain Event Handlers: {DomainEventHandlerCount}\\n\" +");
            sb.AppendLine("                   $\"Total Relationships: {RelationshipCount}\";");
            sb.AppendLine("        }");
            
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GetSimpleTypeName(string fullTypeName)
        {
            var lastDotIndex = fullTypeName.LastIndexOf('.');
            return lastDotIndex >= 0 ? fullTypeName.Substring(lastDotIndex + 1) : fullTypeName;
        }

        #endregion
    }

    #region Enhanced Data Models

    public class TypeAnalysis
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Namespace { get; set; } = "";
        public LocationInfo Location { get; set; } = new();

        public TypeAnalysis() { }

        public TypeAnalysis(INamedTypeSymbol symbol, SyntaxNode declaration)
        {
            Name = symbol.Name;
            FullName = symbol.ToDisplayString();
            Namespace = symbol.ContainingNamespace.ToDisplayString();
            
            var location = declaration.GetLocation();
            var lineSpan = location.GetLineSpan();
            Location = new LocationInfo
            {
                FileName = location.SourceTree?.FilePath ?? "",
                Line = lineSpan.StartLinePosition.Line + 1,
                Column = lineSpan.StartLinePosition.Character + 1
            };
        }
    }

    public class ControllerAnalysis
    {
        public TypeAnalysis Type { get; set; } = new();
        public List<ActionInfo> Actions { get; set; } = new();

        public ControllerAnalysis() { }

        public ControllerAnalysis(TypeAnalysis type, List<ActionInfo> actions)
        {
            Type = type;
            Actions = actions;
        }
    }

    public class ActionInfo
    {
        public string Name { get; set; } = "";
        public string HttpMethod { get; set; } = "";
        public string RouteTemplate { get; set; } = "";
        public List<string> Commands { get; set; } = new();
    }

    public class CommandAnalysis
    {
        public TypeAnalysis Type { get; set; } = new();
        public List<ParameterInfo> Parameters { get; set; } = new();

        public CommandAnalysis() { }

        public CommandAnalysis(TypeAnalysis type, List<ParameterInfo> parameters)
        {
            Type = type;
            Parameters = parameters;
        }
    }

    public class CommandHandlerAnalysis
    {
        public TypeAnalysis Type { get; set; } = new();
        public string HandledCommandType { get; set; } = "";
        public List<string> EntityCalls { get; set; } = new();

        public CommandHandlerAnalysis() { }

        public CommandHandlerAnalysis(TypeAnalysis type, string handledCommandType, List<string> entityCalls)
        {
            Type = type;
            HandledCommandType = handledCommandType;
            EntityCalls = entityCalls;
        }
    }

    public class EntityAnalysis
    {
        public TypeAnalysis Type { get; set; } = new();
        public bool IsAggregateRoot { get; set; }
        public List<EntityMethodInfo> Methods { get; set; } = new();
        public List<string> DomainEvents { get; set; } = new();

        public EntityAnalysis() { }

        public EntityAnalysis(TypeAnalysis type, bool isAggregateRoot, List<EntityMethodInfo> methods, List<string> domainEvents)
        {
            Type = type;
            IsAggregateRoot = isAggregateRoot;
            Methods = methods;
            DomainEvents = domainEvents;
        }
    }

    public class EntityMethodInfo
    {
        public string Name { get; set; } = "";
        public List<string> DomainEvents { get; set; } = new();
    }

    public class DomainEventAnalysis
    {
        public TypeAnalysis Type { get; set; } = new();
        public List<ParameterInfo> Properties { get; set; } = new();
        public string EntityType { get; set; } = "";

        public DomainEventAnalysis() { }

        public DomainEventAnalysis(TypeAnalysis type, List<ParameterInfo> properties, string entityType)
        {
            Type = type;
            Properties = properties;
            EntityType = entityType;
        }
    }

    public class DomainEventHandlerAnalysis
    {
        public TypeAnalysis Type { get; set; } = new();
        public string HandledEventType { get; set; } = "";
        public List<string> Commands { get; set; } = new();

        public DomainEventHandlerAnalysis() { }

        public DomainEventHandlerAnalysis(TypeAnalysis type, string handledEventType, List<string> commands)
        {
            Type = type;
            HandledEventType = handledEventType;
            Commands = commands;
        }
    }

    public class ParameterInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsRequired { get; set; }
    }

    public class MethodInfo
    {
        public string TypeName { get; set; } = "";
        public string MethodName { get; set; } = "";

        public MethodInfo() { }

        public MethodInfo(string typeName, string methodName)
        {
            TypeName = typeName;
            MethodName = methodName;
        }
    }

    public class LocationInfo
    {
        public string FileName { get; set; } = "";
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class CallRelationshipAnalysis
    {
        public MethodInfo Source { get; set; } = new();
        public MethodInfo Target { get; set; } = new();
        public CallType CallType { get; set; }
        public LocationInfo CallLocation { get; set; } = new();
    }

    public class CallChain
    {
        public string Controller { get; set; } = "";
        public string Command { get; set; } = "";
        public string Entity { get; set; } = "";
        public string EntityMethod { get; set; } = "";
        public List<string> DomainEvents { get; set; } = new();
        public List<string> DomainEventHandlers { get; set; } = new();
        public List<string> SubsequentCommands { get; set; } = new();
    }

    public enum CallType
    {
        ControllerToCommand,
        DomainEventHandlerToCommand,
        CommandToEntity,
        EntityToDomainEvent,
        Other
    }

    #endregion
}
