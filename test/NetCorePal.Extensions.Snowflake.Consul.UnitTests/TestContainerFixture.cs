using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Snowflake.Consul.UnitTests
{
    public class TestContainerFixture : IDisposable
    {


        public TestContainerFixture()
        {
            this.ConsulContainer = new ConsulBuilder().Build();
            this.ConsulContainer.StartAsync().Wait();
        }



        public void Dispose()
        {
            this.ConsulContainer.StopAsync().Wait();
        }

        public ConsulContainer ConsulContainer { get; private set; }
    }


    [CollectionDefinition("consul")]
    public class TestContainerFixtureCollection : ICollectionFixture<TestContainerFixture>
    {
    }
}
