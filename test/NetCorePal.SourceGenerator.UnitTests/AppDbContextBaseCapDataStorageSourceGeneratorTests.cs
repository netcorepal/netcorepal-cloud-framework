using Microsoft.CodeAnalysis.CSharp;
using NetCorePal.Extensions.DistributedTransactions.CAP.SourceGenerators;
using Xunit;

namespace NetCorePal.SourceGenerator.UnitTests
{
    public class AppDbContextBaseCapDataStorageSourceGeneratorTests
    {
        [Fact]
        public void GeneratesCapDataStorageForMySqlCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IMySqlCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, IMySqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IMySqlCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("protected override void ConfigureNetCorePalTypes(ModelBuilder modelBuilder)", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(IMySqlCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForSqlServerCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface ISqlServerCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, ISqlServerCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : ISqlServerCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(ISqlServerCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForPostgreSqlCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IPostgreSqlCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, IPostgreSqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IPostgreSqlCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(IPostgreSqlCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForGaussDBCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IGaussDBCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, IGaussDBCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IGaussDBCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(IGaussDBCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForDMDBCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IDMDBCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, IDMDBCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IDMDBCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(IDMDBCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForSqliteCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface ISqliteCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, ISqliteCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : ISqliteCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(ISqliteCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForKingbaseESCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IKingbaseESCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, IKingbaseESCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IKingbaseESCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(IKingbaseESCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForMongoDBCapDataStorage()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IMongoDBCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, IMongoDBCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IMongoDBCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
            Assert.Contains("public DbSet<ReceivedMessage> ReceivedMessages => Set<ReceivedMessage>();", generatedCode);
            Assert.Contains("public DbSet<CapLock> CapLocks => Set<CapLock>();", generatedCode);
            Assert.Contains("protected override void ConfigureNetCorePalTypes(ModelBuilder modelBuilder)", generatedCode);
            Assert.Contains("modelBuilder.ApplyConfigurationsFromAssembly(typeof(IMongoDBCapDataStorage).Assembly);", generatedCode);
        }

        [Fact]
        public void DoesNotGenerateCapDataStorageForAbstractDbContext()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IMySqlCapDataStorage : ICapDataStorage { }
                public abstract class TestDbContext : AppDbContextBase, IMySqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Empty(generatedCode);
        }

        [Fact]
        public void DoesNotGenerateCapDataStorageForNonDbContextBase()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IMySqlCapDataStorage : ICapDataStorage { }
                public class TestDbContext : IMySqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Empty(generatedCode);
        }

        [Fact]
        public void DoesNotGenerateCapDataStorageWithoutCapDataStorageInterface()
        {
            var source = @"
            namespace TestNamespace
            {
                public class TestDbContext : AppDbContextBase { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Empty(generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForIndirectInheritanceFromAppDbContextBase()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IMySqlCapDataStorage : ICapDataStorage { }
                public class IntermediateDbContext : AppDbContextBase { }
                public class TestDbContext : IntermediateDbContext, IMySqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IMySqlCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForAppIdentityDbContextBase()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IMySqlCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppIdentityDbContextBase, IMySqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IMySqlCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
        }

        [Fact]
        public void GeneratesCapDataStorageForAppIdentityUserContextBase()
        {
            var source = @"
            namespace TestNamespace
            {
                public interface ICapDataStorage { }
                public interface IMySqlCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppIdentityUserContextBase, IMySqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("public partial class TestDbContext : IMySqlCapDataStorage", generatedCode);
            Assert.Contains("public DbSet<PublishedMessage> PublishedMessages => Set<PublishedMessage>();", generatedCode);
        }

        [Fact]
        public void GeneratesCorrectNamespaceForCapDataStorage()
        {
            var source = @"
            namespace My.Custom.Namespace
            {
                public interface ICapDataStorage { }
                public interface IMySqlCapDataStorage : ICapDataStorage { }
                public class TestDbContext : AppDbContextBase, IMySqlCapDataStorage { }
            }";

            var generatedCode = RunGenerator(source);

            Assert.Contains("namespace My.Custom.Namespace", generatedCode);
            Assert.Contains("public partial class TestDbContext : IMySqlCapDataStorage", generatedCode);
        }

        private string RunGenerator(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(typeof(AppDbContextBaseCapDataStorageSourceGeneratorTests)
                .Assembly
                .GetName().ToString(), new[] { syntaxTree });
            var generator = new AppDbContextBaseCapDataStorageSourceGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation,
                out var outputCompilation, out var diagnostics);
            var generatedTrees = outputCompilation.SyntaxTrees.Skip(1).ToList();
            return generatedTrees.FirstOrDefault()?.ToString() ?? string.Empty;
        }
    }
}
