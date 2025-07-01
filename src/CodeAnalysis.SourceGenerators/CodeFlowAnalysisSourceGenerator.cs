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
    /// 代码流分析源生成器 - 分析Controller->Command->Entity->DomainEvent->DomainEventHandler的调用链
    /// </summary>
    [Generator]
    public class CodeFlowAnalysisSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 收集所有相关的类型
            var syntaxProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax or MethodDeclarationSyntax,
                    transform: (syntaxContext, _) => syntaxContext)
                .Where(ctx => ctx.Node != null);

            var compilationAndSyntax = context.CompilationProvider.Combine(syntaxProvider.Collect());

            context.RegisterSourceOutput(compilationAndSyntax, (spc, source) =>
            {
                var (compilation, syntaxContexts) = source;
                var analyzer = new CodeFlowAnalyzer(compilation);
                
                // 分析所有语法节点
                foreach (var syntaxContext in syntaxContexts)
                {
                    analyzer.AnalyzeSyntaxNode(syntaxContext);
                }

                // 生成分析结果
                var analysisResult = analyzer.GenerateAnalysisResult();
                spc.AddSource("CodeFlowAnalysis.g.cs", SourceText.From(analysisResult, Encoding.UTF8));
            });
        }
    }

    /// <summary>
    /// 代码流分析器
    /// </summary>
    public class CodeFlowAnalyzer
    {
        private readonly Compilation _compilation;
        private readonly List<ControllerInfo> _controllers = new();
        private readonly List<CommandInfo> _commands = new();
        private readonly List<EntityInfo> _entities = new();
        private readonly List<DomainEventInfo> _domainEvents = new();
        private readonly List<DomainEventHandlerInfo> _domainEventHandlers = new();
        private readonly List<CallRelationship> _relationships = new();

        public CodeFlowAnalyzer(Compilation compilation)
        {
            _compilation = compilation;
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
                case MethodDeclarationSyntax methodDeclaration:
                    AnalyzeMethodDeclaration(methodDeclaration, semanticModel);
                    break;
            }
        }

        private void AnalyzeClassDeclaration(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            if (symbol == null) return;

            var className = symbol.Name;
            var namespaceName = symbol.ContainingNamespace.ToDisplayString();
            var fullName = $"{namespaceName}.{className}";

            // 识别Controller
            if (IsController(symbol))
            {
                _controllers.Add(new ControllerInfo
                {
                    Name = className,
                    FullName = fullName,
                    Methods = GetControllerMethods(classDeclaration, semanticModel)
                });
            }
            // 识别Command
            else if (IsCommand(symbol))
            {
                _commands.Add(new CommandInfo
                {
                    Name = className,
                    FullName = fullName,
                    Properties = GetCommandProperties(classDeclaration, semanticModel)
                });
            }
            // 识别Entity
            else if (IsEntity(symbol))
            {
                _entities.Add(new EntityInfo
                {
                    Name = className,
                    FullName = fullName,
                    Methods = GetEntityMethods(classDeclaration, semanticModel),
                    IsAggregateRoot = IsAggregateRoot(symbol)
                });
            }
            // 识别DomainEvent
            else if (IsDomainEvent(symbol))
            {
                _domainEvents.Add(new DomainEventInfo
                {
                    Name = className,
                    FullName = fullName,
                    Properties = GetDomainEventProperties(classDeclaration, semanticModel)
                });
            }
            // 识别DomainEventHandler
            else if (IsDomainEventHandler(symbol))
            {
                var handledEventType = GetHandledDomainEventType(symbol);
                _domainEventHandlers.Add(new DomainEventHandlerInfo
                {
                    Name = className,
                    FullName = fullName,
                    HandledEventType = handledEventType,
                    Commands = GetDomainEventHandlerCommands(classDeclaration, semanticModel)
                });
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
            var sourceTypeName = containingType.ToDisplayString();
            var sourceMethodName = methodSymbol.Name;

            // 查找方法内的调用
            var invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            
            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol calledMethod)
                {
                    var targetTypeName = calledMethod.ContainingType.ToDisplayString();
                    var targetMethodName = calledMethod.Name;

                    // 记录调用关系
                    _relationships.Add(new CallRelationship
                    {
                        SourceType = sourceTypeName,
                        SourceMethod = sourceMethodName,
                        TargetType = targetTypeName,
                        TargetMethod = targetMethodName,
                        CallType = DetermineCallType(containingType, calledMethod.ContainingType)
                    });

                    // 特殊处理：检查是否是AddDomainEvent调用
                    if (calledMethod.Name == "AddDomainEvent")
                    {
                        var domainEventType = GetDomainEventTypeFromAddCall(invocation, semanticModel);
                        if (!string.IsNullOrEmpty(domainEventType))
                        {
                            _relationships.Add(new CallRelationship
                            {
                                SourceType = sourceTypeName,
                                SourceMethod = sourceMethodName,
                                TargetType = domainEventType,
                                TargetMethod = "Create",
                                CallType = "EntityToDomainEvent"
                            });
                        }
                    }

                    // 特殊处理：检查是否是mediator.Send调用
                    if (calledMethod.Name == "Send" && 
                        calledMethod.ContainingType.Name == "IMediator")
                    {
                        var commandType = GetCommandTypeFromSendCall(invocation, semanticModel);
                        if (!string.IsNullOrEmpty(commandType))
                        {
                            _relationships.Add(new CallRelationship
                            {
                                SourceType = sourceTypeName,
                                SourceMethod = sourceMethodName,
                                TargetType = commandType,
                                TargetMethod = "Handle",
                                CallType = DetermineCallTypeForMediatorSend(containingType)
                            });
                        }
                    }
                }
            }
        }

        private bool IsController(INamedTypeSymbol symbol)
        {
            return symbol.Name.EndsWith("Controller") ||
                   symbol.AllInterfaces.Any(i => i.Name == "IController") ||
                   symbol.BaseType?.Name == "ControllerBase" ||
                   symbol.BaseType?.Name == "Controller";
        }

        private bool IsCommand(INamedTypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "ICommand" || i.Name == "IBaseCommand") ||
                   symbol.Name.EndsWith("Command");
        }

        private bool IsEntity(INamedTypeSymbol symbol)
        {
            return symbol.BaseType?.Name == "Entity" ||
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

        private string GetHandledDomainEventType(INamedTypeSymbol symbol)
        {
            var domainEventHandlerInterface = symbol.AllInterfaces
                .FirstOrDefault(i => i.IsGenericType && i.ConstructedFrom.Name == "IDomainEventHandler");
            
            return domainEventHandlerInterface?.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "";
        }

        private string GetDomainEventTypeFromAddCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            // 从 AddDomainEvent(new SomeDomainEvent(...)) 中提取事件类型
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

        private string GetCommandTypeFromSendCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            // 从 mediator.Send(new SomeCommand(...)) 中提取命令类型
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

        private string DetermineCallType(INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
        {
            if (IsController(sourceType) && IsCommand(targetType))
                return "ControllerToCommand";
            if (IsDomainEventHandler(sourceType) && IsCommand(targetType))
                return "DomainEventHandlerToCommand";
            if (IsEntity(sourceType) && IsDomainEvent(targetType))
                return "EntityToDomainEvent";
            
            return "Other";
        }

        private string DetermineCallTypeForMediatorSend(INamedTypeSymbol sourceType)
        {
            if (IsController(sourceType))
                return "ControllerToCommand";
            if (IsDomainEventHandler(sourceType))
                return "DomainEventHandlerToCommand";
            
            return "Other";
        }

        private List<string> GetControllerMethods(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            return classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.AttributeLists.Any(al => 
                    al.Attributes.Any(a => 
                        a.Name.ToString().Contains("Http") ||
                        a.Name.ToString().Contains("Get") ||
                        a.Name.ToString().Contains("Post") ||
                        a.Name.ToString().Contains("Put") ||
                        a.Name.ToString().Contains("Delete"))))
                .Select(m => m.Identifier.ValueText)
                .ToList();
        }

        private List<string> GetCommandProperties(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            return classDeclaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(p => p.Identifier.ValueText)
                .ToList();
        }

        private List<string> GetEntityMethods(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            return classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                .Select(m => m.Identifier.ValueText)
                .ToList();
        }

        private List<string> GetDomainEventProperties(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            return classDeclaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(p => p.Identifier.ValueText)
                .ToList();
        }

        private List<string> GetDomainEventHandlerCommands(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var commands = new List<string>();
            
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();
                foreach (var invocation in invocations)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                    if (symbolInfo.Symbol is IMethodSymbol methodSymbol &&
                        methodSymbol.Name == "Send" &&
                        methodSymbol.ContainingType.Name == "IMediator")
                    {
                        var commandType = GetCommandTypeFromSendCall(invocation, semanticModel);
                        if (!string.IsNullOrEmpty(commandType))
                        {
                            commands.Add(commandType);
                        }
                    }
                }
            }
            
            return commands;
        }

        public string GenerateAnalysisResult()
        {
            var result = new CodeFlowAnalysisResult
            {
                Controllers = _controllers,
                Commands = _commands,
                Entities = _entities,
                DomainEvents = _domainEvents,
                DomainEventHandlers = _domainEventHandlers,
                Relationships = _relationships
            };

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// 代码流分析结果");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace NetCorePal.Extensions.CodeAnalysis");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 代码流分析结果");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class CodeFlowAnalysisResult");
            sb.AppendLine("    {");
            
            // 生成控制器信息
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 控制器列表");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static readonly List<ControllerInfo> Controllers = new()");
            sb.AppendLine("        {");
            foreach (var controller in _controllers)
            {
                sb.AppendLine($"            new ControllerInfo {{ Name = \"{controller.Name}\", FullName = \"{controller.FullName}\", Methods = new List<string> {{ {string.Join(", ", controller.Methods.Select(m => $"\"{m}\""))} }} }},");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // 生成命令信息
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 命令列表");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static readonly List<CommandInfo> Commands = new()");
            sb.AppendLine("        {");
            foreach (var command in _commands)
            {
                sb.AppendLine($"            new CommandInfo {{ Name = \"{command.Name}\", FullName = \"{command.FullName}\", Properties = new List<string> {{ {string.Join(", ", command.Properties.Select(p => $"\"{p}\""))} }} }},");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // 生成实体信息
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 实体列表");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static readonly List<EntityInfo> Entities = new()");
            sb.AppendLine("        {");
            foreach (var entity in _entities)
            {
                sb.AppendLine($"            new EntityInfo {{ Name = \"{entity.Name}\", FullName = \"{entity.FullName}\", IsAggregateRoot = {entity.IsAggregateRoot.ToString().ToLower()}, Methods = new List<string> {{ {string.Join(", ", entity.Methods.Select(m => $"\"{m}\""))} }} }},");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // 生成领域事件信息
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 领域事件列表");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static readonly List<DomainEventInfo> DomainEvents = new()");
            sb.AppendLine("        {");
            foreach (var domainEvent in _domainEvents)
            {
                sb.AppendLine($"            new DomainEventInfo {{ Name = \"{domainEvent.Name}\", FullName = \"{domainEvent.FullName}\", Properties = new List<string> {{ {string.Join(", ", domainEvent.Properties.Select(p => $"\"{p}\""))} }} }},");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // 生成领域事件处理器信息
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 领域事件处理器列表");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static readonly List<DomainEventHandlerInfo> DomainEventHandlers = new()");
            sb.AppendLine("        {");
            foreach (var handler in _domainEventHandlers)
            {
                sb.AppendLine($"            new DomainEventHandlerInfo {{ Name = \"{handler.Name}\", FullName = \"{handler.FullName}\", HandledEventType = \"{handler.HandledEventType}\", Commands = new List<string> {{ {string.Join(", ", handler.Commands.Select(c => $"\"{c}\""))} }} }},");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // 生成调用关系
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 调用关系列表");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static readonly List<CallRelationship> Relationships = new()");
            sb.AppendLine("        {");
            foreach (var relationship in _relationships)
            {
                sb.AppendLine($"            new CallRelationship {{ SourceType = \"{relationship.SourceType}\", SourceMethod = \"{relationship.SourceMethod}\", TargetType = \"{relationship.TargetType}\", TargetMethod = \"{relationship.TargetMethod}\", CallType = \"{relationship.CallType}\" }},");
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // 生成分析方法
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取控制器调用的命令");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static List<string> GetCommandsCalledByController(string controllerName)");
            sb.AppendLine("        {");
            sb.AppendLine("            return Relationships");
            sb.AppendLine("                .Where(r => r.CallType == \"ControllerToCommand\" && r.SourceType.EndsWith($\".{controllerName}\"))");
            sb.AppendLine("                .Select(r => r.TargetType)");
            sb.AppendLine("                .Distinct()");
            sb.AppendLine("                .ToList();");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取实体方法创建的领域事件");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static List<string> GetDomainEventsCreatedByEntity(string entityName)");
            sb.AppendLine("        {");
            sb.AppendLine("            return Relationships");
            sb.AppendLine("                .Where(r => r.CallType == \"EntityToDomainEvent\" && r.SourceType.EndsWith($\".{entityName}\"))");
            sb.AppendLine("                .Select(r => r.TargetType)");
            sb.AppendLine("                .Distinct()");
            sb.AppendLine("                .ToList();");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 获取领域事件处理器发出的命令");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static List<string> GetCommandsIssuedByDomainEventHandler(string handlerName)");
            sb.AppendLine("        {");
            sb.AppendLine("            return Relationships");
            sb.AppendLine("                .Where(r => r.CallType == \"DomainEventHandlerToCommand\" && r.SourceType.EndsWith($\".{handlerName}\"))");
            sb.AppendLine("                .Select(r => r.TargetType)");
            sb.AppendLine("                .Distinct()");
            sb.AppendLine("                .ToList();");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("    }");
            sb.AppendLine();

            // 添加数据模型类定义
            sb.AppendLine("    // 数据模型类");
            sb.AppendLine("    public class ControllerInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string FullName { get; set; } = \"\";");
            sb.AppendLine("        public List<string> Methods { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class CommandInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string FullName { get; set; } = \"\";");
            sb.AppendLine("        public List<string> Properties { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class EntityInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string FullName { get; set; } = \"\";");
            sb.AppendLine("        public bool IsAggregateRoot { get; set; }");
            sb.AppendLine("        public List<string> Methods { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class DomainEventInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string FullName { get; set; } = \"\";");
            sb.AppendLine("        public List<string> Properties { get; set; } = new();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class DomainEventHandlerInfo");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name { get; set; } = \"\";");
            sb.AppendLine("        public string FullName { get; set; } = \"\";");
            sb.AppendLine("        public string HandledEventType { get; set; } = \"\";");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public class CallRelationship");
            sb.AppendLine("    {");
            sb.AppendLine("        public string SourceType { get; set; } = \"\";");
            sb.AppendLine("        public string TargetType { get; set; } = \"\";");
            sb.AppendLine("        public string CallType { get; set; } = \"\";");
            sb.AppendLine("        public string Location { get; set; } = \"\";");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    // 数据模型类
    public class ControllerInfo
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<string> Methods { get; set; } = new();
    }

    public class CommandInfo
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<string> Properties { get; set; } = new();
    }

    public class EntityInfo
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsAggregateRoot { get; set; }
        public List<string> Methods { get; set; } = new();
    }

    public class DomainEventInfo
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<string> Properties { get; set; } = new();
    }

    public class DomainEventHandlerInfo
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public string HandledEventType { get; set; } = "";
        public List<string> Commands { get; set; } = new();
    }

    public class CallRelationship
    {
        public string SourceType { get; set; } = "";
        public string SourceMethod { get; set; } = "";
        public string TargetType { get; set; } = "";
        public string TargetMethod { get; set; } = "";
        public string CallType { get; set; } = string.Empty;
    }

    public class CodeFlowAnalysisResult
    {
        public List<ControllerInfo> Controllers { get; set; } = new();
        public List<CommandInfo> Commands { get; set; } = new();
        public List<EntityInfo> Entities { get; set; } = new();
        public List<DomainEventInfo> DomainEvents { get; set; } = new();
        public List<DomainEventHandlerInfo> DomainEventHandlers { get; set; } = new();
        public List<CallRelationship> Relationships { get; set; } = new();
    }
}
