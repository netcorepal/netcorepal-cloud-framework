using ABC.Template.Web.Tests;

namespace NetCorePal.Web.UnitTests
{
    public class ProgramTests : IClassFixture<MyWebApplicationFactory>
    {
        private readonly MyWebApplicationFactory _factory;

        public ProgramTests(MyWebApplicationFactory factory)
        {
            _factory = factory;
        }


        [Fact]
        public void HealthCheckTest()
        {
            var client = _factory.CreateClient();
            var response = client.GetAsync("/health").Result;
            Assert.True(response.IsSuccessStatusCode);
        }


        [Fact]
        public async Task SagaTest()
        {
            var client = _factory.CreateClient();
            await Task.Delay(2000);
            var response = client.GetAsync("/saga").Result;
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}