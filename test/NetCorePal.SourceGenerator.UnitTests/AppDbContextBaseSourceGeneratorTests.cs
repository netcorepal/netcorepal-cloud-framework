using Microsoft.CodeAnalysis.CSharp;
using NetCorePal.Extensions.Repository.EntityFrameworkCore.SourceGenerators;
using Xunit;

namespace NetCorePal.SourceGenerator.UnitTests
{
    public class AppDbContextBaseSourceGeneratorTests
    {
        [Fact]
        public void GeneratesValueConverterForStronglyTypedId()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IStronglyTypedId<T> { T Id { get; } }
                public class TestId : IStronglyTypedId<int> { public int Id { get; } }
                public class TestDbContext : AppDbContextBase { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public class TestIdValueConverter", string.Join("\n",generatedCode));
            Assert.Contains("public TestIdValueConverter() : base(p => p.Id, p => new global::TestNamespace.TestId(p))", string.Join("\n",generatedCode));
        }

        [Fact]
        public void DoesNotGenerateValueConverterForNonStronglyTypedId()
        {
            var source = @"
            namespace TestNamespace
            {
                public class TestId { public int Id { get; } }
                public class TestDbContext : AppDbContextBase { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.DoesNotContain("public class TestIdValueConverter", string.Join("\n",generatedCode));
        }

        [Fact]
        public void GeneratesConfigurationForDbContext()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IStronglyTypedId<T> { T Id { get; } }
                public class TestId : IStronglyTypedId<int> { public int Id { get; } }
                public class TestDbContext : AppDbContextBase { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("protected override void ConfigureStronglyTypedIdValueConverter(ModelConfigurationBuilder configurationBuilder)", string.Join("\n",generatedCode));
            Assert.Contains("configurationBuilder.Properties<global::TestNamespace.TestId>().HaveConversion<global::TestNamespace.TestDbContextValueConverters.TestNamespace.TestIdValueConverter>();", string.Join("\n",generatedCode));
        }

        private List<string> RunGenerator(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(typeof(AppDbContextBaseSourceGeneratorTests)
                .Assembly
                .GetName().ToString(), new[] { syntaxTree });
            var generator = new AppDbContextBaseSourceGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation,
                out var outputCompilation, out var diagnostics);
            var generatedTrees = outputCompilation.SyntaxTrees.ToList();
            return generatedTrees.Select(tree => tree.ToString()).ToList();
        }
    }
}