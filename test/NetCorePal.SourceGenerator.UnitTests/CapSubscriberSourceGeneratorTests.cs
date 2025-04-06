using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NetCorePal.Extensions.DistributedTransactions.CAP.SourceGenerators;
using Xunit;

namespace NetCorePal.SourceGenerator.UnitTests
{
    public class CapSubscriberSourceGeneratorTests
    {
        [Fact]
        public void GeneratesSubscriberForValidEventHandler()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IIntegrationEventHandler<T> { }
                public class TestEvent { }
                public class TestHandler : IIntegrationEventHandler<TestEvent> { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public class TestHandlerAsyncSubscriber", generatedCode);
            Assert.Contains("public Task ProcessAsync(TestNamespace.TestEvent message, [FromCap]CapHeader headers, CancellationToken cancellationToken)", generatedCode);
        }

        [Fact]
        public void DoesNotGenerateSubscriberForMissingRootNamespace()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IIntegrationEventHandler<T> { }
                public class TestEvent { }
                public class TestHandler : IIntegrationEventHandler<TestEvent> { }
            }";

            var generatedCode = RunGenerator(source, rootNamespace: null);

            Assert.Empty(generatedCode);
        }

        [Fact]
        public void DoesNotGenerateSubscriberForNonEventHandlerClass()
        {
            var source = @"
            namespace TestNamespace
            {
                public class TestEvent { }
                public class NonEventHandlerClass { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Empty(generatedCode);
        }

        private string RunGenerator(string source, string? rootNamespace = "TestNamespace")
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree });

            var analyzerConfigOptions = new Dictionary<string, string>
            {
                ["build_property.RootNamespace"] = rootNamespace ?? string.Empty
            };

            var driver = CSharpGeneratorDriver.Create(
                new[] { new CapSubscriberSourceGenerator().AsSourceGenerator() },
                parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.First().Options,
                optionsProvider: new CustomAnalyzerConfigOptionsProvider(analyzerConfigOptions));

            driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            var generatedTrees = outputCompilation.SyntaxTrees.Skip(1).ToList();
            return generatedTrees.FirstOrDefault()?.ToString() ?? string.Empty;
        }

        private class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            private readonly Dictionary<string, string> _globalOptions;

            public CustomAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions)
            {
                _globalOptions = globalOptions;
            }

            public override AnalyzerConfigOptions GlobalOptions => new CustomAnalyzerConfigOptions(_globalOptions);

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new CustomAnalyzerConfigOptions(_globalOptions);
            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new CustomAnalyzerConfigOptions(_globalOptions);
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