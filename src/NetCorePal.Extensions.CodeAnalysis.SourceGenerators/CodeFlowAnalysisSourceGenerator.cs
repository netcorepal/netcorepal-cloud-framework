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
    /// 代码关系分析源生成器
    /// </summary>
    [Generator]
    public class CodeFlowAnalysisSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 收集所有相关的类型
            var syntaxProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax or MethodDeclarationSyntax or ConstructorDeclarationSyntax,
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
                spc.AddSource("CodeFlowAnalysisResult.g.cs", SourceText.From(analysisResult, Encoding.UTF8));
            });
        }
    }

    /// <summary>
    /// 代码流分析器
    /// </summary>
    public class CodeFlowAnalyzer
    {
        private readonly Compilation _compilation;
        
        // 发出命令的类型 (Controllers and Endpoints)
        private readonly List<(string Name, string FullName, List<string> Methods)> _commandSenders = new();
        
        // 所有发出命令的类型和方法 (任何调用Send方法的类型)
        private readonly Dictionary<string, (string Name, string FullName, HashSet<string> Methods)> _allCommandSenders = new();
        
        // 聚合
        private readonly List<(string Name, string FullName, List<string> Methods)> _aggregates = new();
        
        // 命令
        private readonly List<(string Name, string FullName)> _commands = new();
        
        // 聚合方法发出的领域事件
        private readonly List<(string Name, string FullName)> _domainEvents = new();
        
        // 集成事件
        private readonly List<(string Name, string FullName)> _integrationEvents = new();
        
        // 领域事件处理器
        private readonly List<(string Name, string FullName, string HandledEventType, List<string> Commands)> _domainEventHandlers = new();
        
        // 集成事件处理器
        private readonly List<(string Name, string FullName, string HandledEventType, List<string> Commands)> _integrationEventHandlers = new();
        
        // 集成事件转换器
        private readonly List<(string Name, string FullName, string DomainEventType, string IntegrationEventType)> _integrationEventConverters = new();
        
        // 关系列表
        private readonly List<(string SourceType, string SourceMethod, string TargetType, string TargetMethod, string RelationType)> _relationships = new();

        public CodeFlowAnalyzer(Compilation compilation)
        {
            _compilation = compilation;
        }

        public void AnalyzeSyntaxNode(GeneratorSyntaxContext syntaxContext)
        {
            var semanticModel = syntaxContext.SemanticModel;
            var node = syntaxContext.Node;

            if (node is ClassDeclarationSyntax classDeclaration)
            {
                AnalyzeClassDeclaration(classDeclaration, semanticModel);
            }
            else if (node is RecordDeclarationSyntax recordDeclaration)
            {
                AnalyzeRecordDeclaration(recordDeclaration, semanticModel);
            }
            else if (node is MethodDeclarationSyntax methodDeclaration)
            {
                AnalyzeMethodDeclaration(methodDeclaration, semanticModel);
            }
            else if (node is ConstructorDeclarationSyntax constructorDeclaration)
            {
                AnalyzeConstructorDeclaration(constructorDeclaration, semanticModel);
            }
        }

        private void AnalyzeClassDeclaration(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol symbol) return;

            var className = symbol.Name;
            var fullName = symbol.ToDisplayString();

            // 1. 发出命令的类型 (Controller, Endpoint, DomainEventHandler, IntegrationEventHandler)
            if (IsController(symbol) || IsEndpoint(symbol) || IsDomainEventHandler(symbol, out _) || IsIntegrationEventHandler(symbol, out _))
            {
                var methods = GetMethodsFromClass(classDeclaration);
                _commandSenders.Add((className, fullName, methods));
            }

            // 3. 聚合
            if (IsAggregate(symbol))
            {
                var methods = GetMethodsFromClass(classDeclaration);
                _aggregates.Add((className, fullName, methods));
            }

            // 命令
            if (IsCommand(symbol))
            {
                _commands.Add((className, fullName));
            }

            // 5. 聚合方法发出的领域事件
            if (IsDomainEvent(symbol))
            {
                _domainEvents.Add((className, fullName));
            }

            // 6. 集成事件
            if (IsIntegrationEvent(symbol))
            {
                _integrationEvents.Add((className, fullName));
            }

            // 7. 领域事件处理器
            if (IsDomainEventHandler(symbol, out var handledDomainEventType))
            {
                _domainEventHandlers.Add((className, fullName, handledDomainEventType, new List<string>()));
                // 关系5: 领域事件与领域事件处理器的关系
                _relationships.Add((handledDomainEventType, "", fullName, "HandleAsync", "DomainEventToHandler"));
            }

            // 8. 集成事件处理器
            if (IsIntegrationEventHandler(symbol, out var handledIntegrationEventType))
            {
                _integrationEventHandlers.Add((className, fullName, handledIntegrationEventType, new List<string>()));
                // 关系6: 集成事件与集成事件处理器的关系
                _relationships.Add((handledIntegrationEventType, "", fullName, "Subscribe", "IntegrationEventToHandler"));
            }

            // 关系4: 领域事件被集成事件转换器转换对应的集成事件的关系
            if (IsIntegrationEventConverter(symbol, out var conversion))
            {
                _integrationEventConverters.Add((symbol.Name, symbol.ToDisplayString(), conversion.domainEvent, conversion.integrationEvent));
                _relationships.Add((conversion.domainEvent, "", conversion.integrationEvent, "", "DomainEventToIntegrationEvent"));
            }
        }

        private void AnalyzeRecordDeclaration(RecordDeclarationSyntax recordDeclaration, SemanticModel semanticModel)
        {
            if (semanticModel.GetDeclaredSymbol(recordDeclaration) is not INamedTypeSymbol symbol) return;

            var className = symbol.Name;
            var fullName = symbol.ToDisplayString();

            // 命令 (record 类型的命令)
            if (IsCommand(symbol))
            {
                _commands.Add((className, fullName));
            }

            // 其他类型的分析 (如果 record 可以实现其他接口)
            // 5. 聚合方法发出的领域事件
            if (IsDomainEvent(symbol))
            {
                _domainEvents.Add((className, fullName));
            }

            // 6. 集成事件
            if (IsIntegrationEvent(symbol))
            {
                _integrationEvents.Add((className, fullName));
            }

            // 7. 领域事件处理器
            if (IsDomainEventHandler(symbol, out var handledDomainEventType))
            {
                _domainEventHandlers.Add((className, fullName, handledDomainEventType, new List<string>()));
                // 关系5: 领域事件与领域事件处理器的关系
                _relationships.Add((handledDomainEventType, "", fullName, "HandleAsync", "DomainEventToHandler"));
            }

            // 8. 集成事件处理器
            if (IsIntegrationEventHandler(symbol, out var handledIntegrationEventType))
            {
                _integrationEventHandlers.Add((className, fullName, handledIntegrationEventType, new List<string>()));
                // 关系6: 集成事件与集成事件处理器的关系
                _relationships.Add((handledIntegrationEventType, "", fullName, "Subscribe", "IntegrationEventToHandler"));
            }

            // 关系4: 领域事件被集成事件转换器转换对应的集成事件的关系
            if (IsIntegrationEventConverter(symbol, out var conversion))
            {
                _integrationEventConverters.Add((symbol.Name, symbol.ToDisplayString(), conversion.domainEvent, conversion.integrationEvent));
                _relationships.Add((conversion.domainEvent, "", conversion.integrationEvent, "", "DomainEventToIntegrationEvent"));
            }
        }

        private void AnalyzeMethodDeclaration(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            if (semanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol) return;

            var containingType = methodSymbol.ContainingType;
            var sourceTypeId = containingType.ToDisplayString();
            var sourceMethodName = methodSymbol.Name;

            bool isSender = IsSenderType(containingType);
            bool isAggregate = IsAggregate(containingType);

            // 查找方法内的调用
            var invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol calledMethod) continue;

                var targetType = calledMethod.ContainingType;
                var targetTypeId = targetType.ToDisplayString();

                // 关系1: 发出命令的方法与命令的关系 - 记录任何发送命令的类型和方法
                if (IsSendMethod(calledMethod))
                {
                    var commandType = GetCommandTypeFromSendCall(invocation, semanticModel);
                    if (!string.IsNullOrEmpty(commandType))
                    {
                        _relationships.Add((sourceTypeId, sourceMethodName, commandType, "", "MethodToCommand"));
                        
                        // 记录发送命令的类型和方法到通用集合中
                        if (!_allCommandSenders.ContainsKey(sourceTypeId))
                        {
                            _allCommandSenders[sourceTypeId] = (containingType.Name, sourceTypeId, new HashSet<string>());
                        }
                        _allCommandSenders[sourceTypeId].Methods.Add(sourceMethodName);
                        
                        // 同时将命令添加到事件处理器的Commands列表中
                        if (IsDomainEventHandler(containingType, out _))
                        {
                            var domainHandler = _domainEventHandlers.FirstOrDefault(h => h.FullName == sourceTypeId);
                            if (domainHandler != default)
                            {
                                var index = _domainEventHandlers.IndexOf(domainHandler);
                                var updatedHandler = (domainHandler.Name, domainHandler.FullName, domainHandler.HandledEventType, 
                                    domainHandler.Commands.Union(new[] { commandType }).ToList());
                                _domainEventHandlers[index] = updatedHandler;
                            }
                        }
                        else if (IsIntegrationEventHandler(containingType, out _))
                        {
                            var integrationHandler = _integrationEventHandlers.FirstOrDefault(h => h.FullName == sourceTypeId);
                            if (integrationHandler != default)
                            {
                                var index = _integrationEventHandlers.IndexOf(integrationHandler);
                                var updatedHandler = (integrationHandler.Name, integrationHandler.FullName, integrationHandler.HandledEventType, 
                                    integrationHandler.Commands.Union(new[] { commandType }).ToList());
                                _integrationEventHandlers[index] = updatedHandler;
                            }
                        }
                    }
                }
                // 关系2: 命令对应的处理器与其调用的聚合方法的关系
                else if (IsCommandHandler(containingType) && IsAggregate(targetType))
                {
                    var commandTypeSymbol = GetCommandTypeFromHandler(containingType);
                    if (commandTypeSymbol != null)
                    {
                        _relationships.Add((commandTypeSymbol.ToDisplayString(), "Handle", targetTypeId, calledMethod.Name, "CommandToAggregateMethod"));
                    }
                }
                // 关系3: 聚合方法与其发出的领域事件的关系
                else if (isAggregate && IsAddDomainEventMethod(calledMethod))
                {
                    var domainEventType = GetDomainEventTypeFromAddCall(invocation, semanticModel);
                    if (!string.IsNullOrEmpty(domainEventType))
                    {
                        _relationships.Add((sourceTypeId, sourceMethodName, domainEventType, "", "MethodToDomainEvent"));
                    }
                }
            }

            // 查找方法内的构造函数调用
            var objectCreations = methodDeclaration.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();

            foreach (var objectCreation in objectCreations)
            {
                if (semanticModel.GetSymbolInfo(objectCreation).Symbol is not IMethodSymbol constructor) continue;

                var targetType = constructor.ContainingType;
                var targetTypeId = targetType.ToDisplayString();

                // 关系2: 命令对应的处理器与其调用的聚合构造函数的关系
                if (IsCommandHandler(containingType) && IsAggregate(targetType))
                {
                    var commandTypeSymbol = GetCommandTypeFromHandler(containingType);
                    if (commandTypeSymbol != null)
                    {
                        _relationships.Add((commandTypeSymbol.ToDisplayString(), "Handle", targetTypeId, ".ctor", "CommandToAggregateMethod"));
                    }
                }
            }

            // 查找方法内的成员访问表达式（用于静态方法调用）
            var memberAccesses = methodDeclaration.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

            foreach (var memberAccess in memberAccesses)
            {
                // 检查是否是静态方法调用
                if (memberAccess.Parent is InvocationExpressionSyntax staticInvocation)
                {
                    if (semanticModel.GetSymbolInfo(staticInvocation).Symbol is not IMethodSymbol staticMethod) continue;
                    
                    // 跳过已经在普通方法调用中处理过的
                    if (!staticMethod.IsStatic) continue;

                    var targetType = staticMethod.ContainingType;
                    var targetTypeId = targetType.ToDisplayString();

                    // 关系2: 命令对应的处理器与其调用的聚合静态方法的关系
                    if (IsCommandHandler(containingType) && IsAggregate(targetType))
                    {
                        var commandTypeSymbol = GetCommandTypeFromHandler(containingType);
                        if (commandTypeSymbol != null)
                        {
                            _relationships.Add((commandTypeSymbol.ToDisplayString(), "Handle", targetTypeId, staticMethod.Name, "CommandToAggregateMethod"));
                        }
                    }
                }
            }
        }

        private void AnalyzeConstructorDeclaration(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            if (semanticModel.GetDeclaredSymbol(constructorDeclaration) is not IMethodSymbol constructorSymbol) return;

            var containingType = constructorSymbol.ContainingType;
            var sourceTypeId = containingType.ToDisplayString();
            var sourceMethodName = ".ctor";

            bool isAggregate = IsAggregate(containingType);

            // 查找构造函数内的调用（发出的领域事件）
            var invocations = constructorDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol calledMethod) continue;

                // 关系3: 聚合构造函数与其发出的领域事件的关系
                if (isAggregate && IsAddDomainEventMethod(calledMethod))
                {
                    var domainEventType = GetDomainEventTypeFromAddCall(invocation, semanticModel);
                    if (!string.IsNullOrEmpty(domainEventType))
                    {
                        _relationships.Add((sourceTypeId, sourceMethodName, domainEventType, "", "MethodToDomainEvent"));
                    }
                }
            }
        }

        private List<string> GetMethodsFromClass(ClassDeclarationSyntax classDeclaration)
        {
            var methods = new List<string>();
            
            // 添加公共实例方法
            methods.AddRange(classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)) &&
                           !m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.OverrideKeyword) && 
                                                 (m.Identifier.Text == "Configure" || m.Identifier.Text == "ToString" || 
                                                  m.Identifier.Text == "GetHashCode" || m.Identifier.Text == "Equals")))
                .Select(m => m.Identifier.Text));
            
            // 添加公共静态方法
            methods.AddRange(classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)) && 
                           m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.StaticKeyword)))
                .Select(m => m.Identifier.Text));
            
            // 添加公共构造函数
            methods.AddRange(classDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Where(c => c.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                .Select(_ => ".ctor"));
                
            return methods.Distinct().ToList();
        }

        private bool IsController(INamedTypeSymbol symbol) => 
            symbol.Name.EndsWith("Controller") || 
            symbol.BaseType?.Name == "ControllerBase" ||
            symbol.BaseType?.Name == "Controller";

        private bool IsEndpoint(INamedTypeSymbol symbol)
        {
            if (symbol.Name.EndsWith("Endpoint"))
            {
                // 检查是否继承自 FastEndpoints 的 Endpoint 基类
                var baseType = symbol.BaseType;
                while (baseType != null)
                {
                    if (baseType.Name == "Endpoint" || 
                        baseType.Name == "EndpointWithoutRequest" ||
                        baseType.Name == "EndpointWithoutResponse")
                    {
                        return true;
                    }
                    baseType = baseType.BaseType;
                }
            }
            return false;
        }

        private bool IsSenderType(INamedTypeSymbol symbol)
        {
            return IsController(symbol) ||
                   IsEndpoint(symbol) ||
                   IsDomainEventHandler(symbol, out _) ||
                   IsIntegrationEventHandler(symbol, out _);
        }

        private bool IsSendMethod(IMethodSymbol symbol)
        {
            return (symbol.Name == "Send" || symbol.Name == "SendAsync") &&
                   (symbol.ContainingType.ToDisplayString().Contains("MediatR") ||
                    symbol.ContainingType.Name.Contains("Mediator"));
        }

        private bool IsAddDomainEventMethod(IMethodSymbol symbol)
        {
            return symbol.Name == "AddDomainEvent" || symbol.Name == "RaiseDomainEvent";
        }

        private bool IsCommandHandler(INamedTypeSymbol symbol) => 
            symbol.AllInterfaces.Any(i =>
                i.IsGenericType && (i.ConstructedFrom.Name == "IRequestHandler" ||
                                    i.ConstructedFrom.Name == "ICommandHandler"));

        private INamedTypeSymbol? GetCommandTypeFromHandler(INamedTypeSymbol handlerSymbol) =>
            handlerSymbol.AllInterfaces.FirstOrDefault(i =>
                i.IsGenericType && (i.ConstructedFrom.Name == "IRequestHandler" ||
                                    i.ConstructedFrom.Name == "ICommandHandler"))?.TypeArguments.FirstOrDefault() as
                INamedTypeSymbol;

        private bool IsIntegrationEvent(INamedTypeSymbol symbol)
        {
            var allTypes = _compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type).Cast<INamedTypeSymbol>();
            
            // 方式1: 检查是否被集成事件处理器处理
            bool handledByHandler = allTypes.Any(type => 
                type.AllInterfaces.Any(i => 
                    i.IsGenericType && 
                    i.ConstructedFrom.Name == "IIntegrationEventHandler" &&
                    i.TypeArguments.Any(arg => SymbolEqualityComparer.Default.Equals(arg, symbol))));
            
            // 方式2: 检查是否由集成事件转换器生成
            bool generatedByConverter = allTypes.Any(type =>
                type.AllInterfaces.Any(i =>
                    i.IsGenericType &&
                    i.Name == "IIntegrationEventConverter" &&
                    i.TypeArguments.Length == 2 &&
                    SymbolEqualityComparer.Default.Equals(i.TypeArguments[1], symbol)));
            
            return handledByHandler || generatedByConverter;
        }

        private bool IsIntegrationEventConverter(INamedTypeSymbol symbol,
            out (string domainEvent, string integrationEvent) conversion)
        {
            conversion = default;
            var converterInterface =
                symbol.AllInterfaces.FirstOrDefault(i =>
                    i.IsGenericType && i.Name == "IIntegrationEventConverter");
            if (converterInterface != null && converterInterface.TypeArguments.Length == 2)
            {
                conversion = (converterInterface.TypeArguments[0].ToDisplayString(),
                    converterInterface.TypeArguments[1].ToDisplayString());
                return true;
            }

            return false;
        }

        private bool IsCommand(INamedTypeSymbol symbol)
        {
            // 排除处理器和锁类 - 这些不是命令本身
            if (symbol.Name.EndsWith("Handler") || symbol.Name.EndsWith("Lock") || symbol.Name.EndsWith("Validator"))
                return false;
                
            // 检查是否实现了 ICommand、ICommand<T> 或 IBaseCommand 接口
            return symbol.AllInterfaces.Any(iface => 
            {
                var name = iface.Name;
                var fullName = iface.ToDisplayString();
                
                // 检查名称匹配 - 包括泛型版本
                return name == "ICommand" || name == "IBaseCommand" || 
                       (iface.IsGenericType && iface.ConstructedFrom?.Name == "ICommand") ||
                       (name == "IRequest" && fullName.Contains("MediatR"));
            });
        }

        private bool IsCommandLock(INamedTypeSymbol symbol)
        {
            // 检查是否是命令锁 - 通常以Lock结尾且实现ICommandLock接口
            if (!symbol.Name.EndsWith("Lock"))
                return false;
                
            return symbol.AllInterfaces.Any(iface => 
            {
                var name = iface.Name;
                var fullName = iface.ToDisplayString();
                
                return name == "ICommandLock" || fullName.Contains("ICommandLock");
            });
        }

        private bool IsAggregate(INamedTypeSymbol symbol) => 
            symbol.AllInterfaces.Any(i => i.Name == "IAggregateRoot") ||
            symbol.BaseType?.AllInterfaces.Any(i => i.Name == "IAggregateRoot") == true;
            
        private bool IsDomainEvent(INamedTypeSymbol symbol) => 
            symbol.AllInterfaces.Any(i => i.Name == "IDomainEvent");

        private bool IsDomainEventHandler(INamedTypeSymbol symbol, out string handledEventType)
        {
            handledEventType = "";
            var handlerInterface =
                symbol.AllInterfaces.FirstOrDefault(i =>
                    i.IsGenericType && i.ConstructedFrom.Name == "IDomainEventHandler");
            if (handlerInterface != null)
            {
                handledEventType = handlerInterface.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "";
                return true;
            }

            return false;
        }

        private bool IsIntegrationEventHandler(INamedTypeSymbol symbol, out string handledEventType)
        {
            handledEventType = "";
            var handlerInterface = symbol.AllInterfaces.FirstOrDefault(i =>
                i.IsGenericType && i.ConstructedFrom.Name == "IIntegrationEventHandler");
            if (handlerInterface != null)
            {
                handledEventType = handlerInterface.TypeArguments.FirstOrDefault()?.ToDisplayString() ?? "";
                return true;
            }

            return false;
        }

        private string GetDomainEventTypeFromAddCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (invocation.ArgumentList.Arguments.FirstOrDefault()
                    ?.Expression is ObjectCreationExpressionSyntax objectCreation)
            {
                return semanticModel.GetTypeInfo(objectCreation).Type?.ToDisplayString() ?? "";
            }

            return "";
        }

        private string GetCommandTypeFromSendCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var argument = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (argument != null)
            {
                return semanticModel.GetTypeInfo(argument).Type?.ToDisplayString() ?? "";
            }

            return "";
        }

        private bool IsControllerByName(string fullName)
        {
            return (fullName.Contains("Controller") || fullName.Contains("Endpoint")) && !fullName.Contains("Handler");
        }

        public string GenerateAnalysisResult()
        {
            // 从集成事件处理器和转换器中推断集成事件类型
            var inferredIntegrationEvents = new HashSet<string>();
            
            // 从集成事件处理器中推断
            foreach (var handler in _integrationEventHandlers)
            {
                var eventType = handler.HandledEventType;
                if (!string.IsNullOrEmpty(eventType))
                {
                    inferredIntegrationEvents.Add(eventType);
                }
            }
            
            // 从集成事件转换器中推断
            foreach (var converter in _integrationEventConverters)
            {
                var eventType = converter.IntegrationEventType;
                if (!string.IsNullOrEmpty(eventType))
                {
                    inferredIntegrationEvents.Add(eventType);
                }
            }
            
            // 添加推断出的集成事件到列表中（避免重复）
            foreach (var eventType in inferredIntegrationEvents)
            {
                var lastDotIndex = eventType.LastIndexOf('.');
                var name = lastDotIndex >= 0 ? eventType.Substring(lastDotIndex + 1) : eventType;
                
                if (!_integrationEvents.Any(e => e.FullName == eventType))
                {
                    _integrationEvents.Add((name, eventType));
                }
            }
            
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using NetCorePal.Extensions.CodeAnalysis;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace NetCorePal.Extensions.CodeAnalysis.SourceGenerators.Result");
            sb.AppendLine("{");
            sb.AppendLine("    public class GeneratedAnalysisResult : IAnalysisResult");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly CodeFlowAnalysisResult _result = new() ");
            sb.AppendLine("        {");

            // 发出命令的类型 (Controllers and Endpoints)
            sb.AppendLine("            Controllers = new List<ControllerInfo> {");
            foreach (var sender in _commandSenders.Where(s => IsControllerByName(s.FullName)))
            {
                sb.AppendLine($"                new ControllerInfo {{ Name = \"{EscapeString(sender.Name)}\", FullName = \"{EscapeString(sender.FullName)}\", Methods = new List<string> {{ {string.Join(", ", sender.Methods.Select(m => $"\"{EscapeString(m)}\""))} }} }},");
            }
            sb.AppendLine("            },");

            // 所有发出命令的类型 (任何调用Send方法的类型)
            sb.AppendLine("            CommandSenders = new List<CommandSenderInfo> {");
            // 调试输出
            sb.AppendLine($"                // Debug: _allCommandSenders count: {_allCommandSenders.Count}");
            foreach (var kvp in _allCommandSenders)
            {
                sb.AppendLine($"                // Debug: {kvp.Key} -> IsController: {IsControllerByName(kvp.Value.FullName)}");
            }
            foreach (var sender in _allCommandSenders.Values)
            {
                // 只包含非控制器类型
                if (!IsControllerByName(sender.FullName))
                {
                    sb.AppendLine($"                new CommandSenderInfo {{ Name = \"{EscapeString(sender.Name)}\", FullName = \"{EscapeString(sender.FullName)}\", Methods = new List<string> {{ {string.Join(", ", sender.Methods.Select(m => $"\"{EscapeString(m)}\""))} }} }},");
                }
            }
            sb.AppendLine("            },");

            // 命令
            sb.AppendLine("            Commands = new List<CommandInfo> {");
            foreach (var command in _commands)
            {
                sb.AppendLine($"                new CommandInfo {{ Name = \"{EscapeString(command.Name)}\", FullName = \"{EscapeString(command.FullName)}\" }},");
            }
            sb.AppendLine("            },");

            // 聚合
            sb.AppendLine("            Entities = new List<EntityInfo> {");
            foreach (var aggregate in _aggregates)
            {
                sb.AppendLine($"                new EntityInfo {{ Name = \"{EscapeString(aggregate.Name)}\", FullName = \"{EscapeString(aggregate.FullName)}\", IsAggregateRoot = true, Methods = new List<string> {{ {string.Join(", ", aggregate.Methods.Select(m => $"\"{EscapeString(m)}\""))} }} }},");
            }
            sb.AppendLine("            },");

            // 领域事件
            sb.AppendLine("            DomainEvents = new List<DomainEventInfo> {");
            foreach (var domainEvent in _domainEvents)
            {
                sb.AppendLine($"                new DomainEventInfo {{ Name = \"{EscapeString(domainEvent.Name)}\", FullName = \"{EscapeString(domainEvent.FullName)}\" }},");
            }
            sb.AppendLine("            },");

            // 集成事件
            sb.AppendLine("            IntegrationEvents = new List<IntegrationEventInfo> {");
            foreach (var integrationEvent in _integrationEvents)
            {
                sb.AppendLine($"                new IntegrationEventInfo {{ Name = \"{EscapeString(integrationEvent.Name)}\", FullName = \"{EscapeString(integrationEvent.FullName)}\" }},");
            }
            sb.AppendLine("            },");

            // 领域事件处理器
            sb.AppendLine("            DomainEventHandlers = new List<DomainEventHandlerInfo> {");
            foreach (var handler in _domainEventHandlers)
            {
                sb.AppendLine($"                new DomainEventHandlerInfo {{ Name = \"{EscapeString(handler.Name)}\", FullName = \"{EscapeString(handler.FullName)}\", HandledEventType = \"{EscapeString(handler.HandledEventType)}\", Commands = new List<string> {{ {string.Join(", ", handler.Commands.Select(c => $"\"{EscapeString(c)}\""))} }} }},");
            }
            sb.AppendLine("            },");

            // 集成事件处理器
            sb.AppendLine("            IntegrationEventHandlers = new List<IntegrationEventHandlerInfo> {");
            foreach (var handler in _integrationEventHandlers)
            {
                sb.AppendLine($"                new IntegrationEventHandlerInfo {{ Name = \"{EscapeString(handler.Name)}\", FullName = \"{EscapeString(handler.FullName)}\", HandledEventType = \"{EscapeString(handler.HandledEventType)}\", Commands = new List<string> {{ {string.Join(", ", handler.Commands.Select(c => $"\"{EscapeString(c)}\""))} }} }},");
            }
            sb.AppendLine("            },");

            // 集成事件转换器
            sb.AppendLine("            IntegrationEventConverters = new List<IntegrationEventConverterInfo> {");
            foreach (var converter in _integrationEventConverters)
            {
                sb.AppendLine($"                new IntegrationEventConverterInfo {{ Name = \"{EscapeString(converter.Name)}\", FullName = \"{EscapeString(converter.FullName)}\", DomainEventType = \"{EscapeString(converter.DomainEventType)}\", IntegrationEventType = \"{EscapeString(converter.IntegrationEventType)}\" }},");
            }
            sb.AppendLine("            },");

            // 关系
            sb.AppendLine("            Relationships = new List<CallRelationship> {");
            foreach (var relationship in _relationships)
            {
                sb.AppendLine($"                new CallRelationship(\"{EscapeString(relationship.SourceType)}\", \"{EscapeString(relationship.SourceMethod)}\", \"{EscapeString(relationship.TargetType)}\", \"{EscapeString(relationship.TargetMethod)}\", \"{EscapeString(relationship.RelationType)}\"),");
            }
            sb.AppendLine("            }");

            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        public CodeFlowAnalysisResult GetResult() => _result;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string EscapeString(string input)
        {
            return input.Replace("\"", "\\\"").Replace("\\", "\\\\");
        }
    }
}
