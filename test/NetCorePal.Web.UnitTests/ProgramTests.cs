using NetCorePal.Extensions.AspNetCore;
using NetCorePal.Extensions.Domain.Json;
using NetCorePal.Web.Application.Commands;
using NetCorePal.Web.Domain;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Web.Application.IntegrationEventHandlers;

namespace NetCorePal.Web.UnitTests
{
    public class ProgramTests : IClassFixture<MyWebApplicationFactory>
    {
        MyWebApplicationFactory factory;

        public ProgramTests(MyWebApplicationFactory factory)
        {
            this.factory = factory;
            JsonOption = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            JsonOption.Converters.Add(new EntityIdJsonConverterFactory());
        }

        JsonSerializerOptions JsonOption;

        [Fact]
        public void HealthCheckTest()
        {
            var client = factory.CreateClient();
            var response = client.GetAsync("/health").Result;
            Assert.True(response.IsSuccessStatusCode);
        }


        //[Fact]
        public async Task SagaTest()
        {
            var client = factory.CreateClient();
            var response = client.GetAsync("/saga").Result;
            Assert.True(response.IsSuccessStatusCode);
        }


        [Fact]
        public async Task KnownExceptionTest()
        {
            var client = factory.CreateClient();
            var response = client.GetAsync("/knownexception").Result;
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("test known exception message", data.Message);
            Assert.Equal(33, data.Code);
            Assert.False(data.Success);
        }


        [Fact]
        public async Task UnknownExceptionTest()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/unknownexception");
            Assert.True(!response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("未知错误", data.Message);
            Assert.Equal(99999, data.Code);
            Assert.False(data.Success);
        }

        [Fact]
        public async Task PostTest()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderCommand("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);


            response = await client.GetAsync($"/get/{data.Id}");
            Assert.True(response.IsSuccessStatusCode);
            var order = await response.Content.ReadFromJsonAsync<Order>(JsonOption);
            Assert.NotNull(order);
            Assert.Equal("na", order.Name);
            Assert.Equal(14, order.Count);

            response = await client.GetAsync($"/sendEvent?id={data.Id}");
            Assert.True(response.IsSuccessStatusCode);
            await Task.Delay(1000); //等待事件处理完成
        }
    }
}