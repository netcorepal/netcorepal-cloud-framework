using Testcontainers.Redis;

namespace NetCorePal.Extensions.Snowflake.Redis.UnitTests
{
    public class TestContainerFixture : IDisposable
    {


        public TestContainerFixture()
        {
            this.RedisContainer = new RedisBuilder().Build();
            this.RedisContainer.StartAsync().Wait();
        }



        public void Dispose()
        {
            this.RedisContainer.StopAsync().Wait();
        }

        public RedisContainer RedisContainer { get; private set; }
    }


    [CollectionDefinition("redis")]
    public class TestContainerFixtureCollection : ICollectionFixture<TestContainerFixture>
    {
    }
}
