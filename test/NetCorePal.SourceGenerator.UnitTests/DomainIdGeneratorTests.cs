using Microsoft.CodeAnalysis.CSharp;
using NetCorePal.Extensions.Domain.SourceGenerators;
using Xunit;

namespace NetCorePal.SourceGenerator.UnitTests
{
    public class EntityIdCodeGeneratorsTests
    {
        [Fact]
        public void GeneratesCodeForInt64StronglyTypedId()
        {
            var source = """
                         namespace TestNamespace
                         {
                             public interface IInt64StronglyTypedId { }
                             public class TestEntity : IInt64StronglyTypedId { }
                         }
                         """;

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial record TestEntity(Int64 Id) : IInt64StronglyTypedId", generatedCode);
        }

        [Fact]
        public void GeneratesCodeForInt32StronglyTypedId()
        {
            var source = """
                         
                                     namespace TestNamespace
                                     {
                                         public interface IInt32StronglyTypedId { }
                                         public class TestEntity : IInt32StronglyTypedId { }
                                     }
                         """;

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial record TestEntity(Int32 Id) : IInt32StronglyTypedId", generatedCode);
        }

        [Fact]
        public void GeneratesCodeForStringStronglyTypedId()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IStringStronglyTypedId { }
                public class TestEntity : IStringStronglyTypedId { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial record TestEntity(String Id) : IStringStronglyTypedId", generatedCode);
        }

        [Fact]
        public void GeneratesCodeForGuidStronglyTypedId()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface IGuidStronglyTypedId { }
                public class TestEntity : IGuidStronglyTypedId { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial record TestEntity(Guid Id) : IGuidStronglyTypedId", generatedCode);
        }

        private string RunGenerator(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation =
                CSharpCompilation.Create(
                    typeof(EntityIdCodeGeneratorsTests)
                        .Assembly
                        .GetName().ToString(), new[] { syntaxTree });
            var generator = new EntityIdCodeGenerators();
            var driver = CSharpGeneratorDriver.Create(generator);

            driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation,
                out var diagnostics);
            var generatedTrees = outputCompilation.SyntaxTrees.Skip(1).ToList();
            return generatedTrees.First().ToString();
        }
    }
}