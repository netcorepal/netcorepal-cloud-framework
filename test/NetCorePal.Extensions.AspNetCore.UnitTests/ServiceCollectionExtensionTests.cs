using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.AspNetCore.CommandLocks;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.UnitTests
{

    public class Query1 : IQuery
    {
    }


    public class ServiceCollectionExtensionTests
    {
        [Fact]
        public void AddAllQueriesTests()
        {
            var services = new ServiceCollection();
            services.AddAllQueries(typeof(ServiceCollectionExtensionTests).Assembly);
            Assert.Single(services);
            var provider = services.BuildServiceProvider();
            var queryHandler = provider.GetRequiredService<Query1>();
            Assert.NotNull(queryHandler);

            using var scope = provider.CreateScope();
            var queryHandler2 = scope.ServiceProvider.GetRequiredService<Query1>();
            var queryHandler3 = scope.ServiceProvider.GetRequiredService<Query1>();
            Assert.Same(queryHandler2, queryHandler3);

            using var scope2 = provider.CreateScope();
            var queryHandler4 = scope2.ServiceProvider.GetRequiredService<Query1>();
            Assert.NotSame(queryHandler2, queryHandler4);
        }
        
        [Fact]
        public void AddCommandLocksTest()
        {
            var services = new ServiceCollection();
            services.AddCommandLocks(typeof(CommandLockBehaviorTest).Assembly);
            var provider = services.BuildServiceProvider();
            var s = provider.GetRequiredService<ICommandLock<CommandLockBehaviorTest.TestCommand>>();
            Assert.NotNull(s);
        }
    }
}
