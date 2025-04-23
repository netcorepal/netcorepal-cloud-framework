using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NetCorePal.Extensions.DistributedTransactions.CAP.SourceGenerators;
using Xunit;

namespace NetCorePal.SourceGenerator.UnitTests
{
    public class CapIntegrationConvertDomainEventHandlerSourceGeneratorTests
    {
        [Fact]
        public void GeneratesCodeForValidIntegrationEventConverter()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IIntegrationEventConverter<T> { }
                public class TestEvent { }
                public class TestConverter : IIntegrationEventConverter<TestEvent> { }
            }";

            var generatedCode = RunGenerator(source,"TestNamespace");

            Assert.Contains("public class TestConverterDomainEventHandler", generatedCode);
            Assert.Contains("public Task Handle(global::TestNamespace.TestEvent notification, CancellationToken cancellationToken)",
                generatedCode);
        }

        [Fact]
        public void DoesNotGenerateCodeForMissingRootNamespace()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IIntegrationEventConverter<T> { }
                public class TestEvent { }
                public class TestConverter : IIntegrationEventConverter<TestEvent> { }
            }";

            var generatedCode = RunGenerator(source, rootNamespace: null);

            Assert.Empty(generatedCode);
        }

        [Fact]
        public void DoesNotGenerateCodeForNonConverterClass()
        {
            var source = @"
            namespace TestNamespace
            {
                public class TestEvent { }
                public class NonConverterClass { }
            }";

            var generatedCode = RunGenerator(source,"TestNamespace");

            Assert.Empty(generatedCode);
        }

        private string RunGenerator(string source, string? rootNamespace)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(this.GetType().Assembly.GetName().Name, new[] { syntaxTree });

            // 创建分析器配置选项
            var analyzerConfigOptions = new Dictionary<string, string>
            {
                ["build_property.RootNamespace"] = rootNamespace ?? string.Empty
            };

            var driver = CSharpGeneratorDriver.Create(
                [new CapIntegrationConvertDomainEventHandlerSourceGenerator().AsSourceGenerator()
                ],
                parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.First().Options,
                optionsProvider: new CustomAnalyzerConfigOptionsProvider(analyzerConfigOptions));

            driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation,
                out var outputCompilation, out var diagnostics);
            var generatedTrees = outputCompilation.SyntaxTrees.Skip(1).ToList();
            return generatedTrees.FirstOrDefault()?.ToString() ?? string.Empty;
        }


// 添加一个自定义的 AnalyzerConfigOptionsProvider 类
        private class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            private readonly Dictionary<string, string> _globalOptions;

            public CustomAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions)
            {
                _globalOptions = globalOptions;
            }

            public override AnalyzerConfigOptions GlobalOptions => new CustomAnalyzerConfigOptions(_globalOptions);

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
                new CustomAnalyzerConfigOptions(_globalOptions);

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
                new CustomAnalyzerConfigOptions(_globalOptions);
        }


        private class CustomAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            private readonly Dictionary<string, string> _options;

            public CustomAnalyzerConfigOptions(Dictionary<string, string> options)
            {
                _options = options;
            }

            public override bool TryGetValue(string key, out string value)
                => _options.TryGetValue(key, out value!);
        }
    }
}