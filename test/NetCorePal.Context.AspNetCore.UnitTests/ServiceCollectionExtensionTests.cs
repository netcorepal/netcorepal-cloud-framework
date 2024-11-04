using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Context.AspNetCore.UnitTests
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
            Assert.Equal(1, services.Count);
            var provider = services.BuildServiceProvider();
            var queryHandler = provider.GetRequiredService<Query1>();
            Assert.NotNull(queryHandler);
        }
    }
}
