using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Dto;
using NetCorePal.Web.Application.Queries;
using NetCorePal.Web.Controllers;
using NetCorePal.Web.Controllers.Request;
using NetCorePal.Web.Domain;
using NetCorePal.Web.Infra;

namespace NetCorePal.Web.UnitTests
{
    public class ProgramTests : IClassFixture<MyWebApplicationFactory>
    {
        MyWebApplicationFactory factory;

        public ProgramTests(MyWebApplicationFactory factory)
        {
            this.factory = factory;
            JsonOption = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            JsonOption.AddNetCorePalJsonConverters();
        }

        JsonSerializerOptions JsonOption;

        [Fact]
        public async Task HealthCheckTest()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/health");
            Assert.True(response.IsSuccessStatusCode);
        }


        //[Fact]
        // public async Task SagaTest()
        // {
        //     var client = factory.CreateClient();
        //     var response = await client.GetAsync("/saga");
        //     Assert.True(response.IsSuccessStatusCode);
        // }


        [Fact]
        public async Task KnownExceptionTest()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/knownexception");
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
        public async Task ServiceKnownExceptionTest()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/service/knownexception");
            Assert.True(!response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("test known exception message", data.Message);
            Assert.Equal(33, data.Code);
            Assert.False(data.Success);
        }


        [Fact]
        public async Task ServiceUnknownExceptionTest()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/service/unknownexception");
            Assert.True(!response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("未知错误", data.Message);
            Assert.Equal(99999, data.Code);
            Assert.False(data.Success);
        }

        [Fact]
        public async Task BadRequestTest()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/badrequest/{abc}");
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("The value '{abc}' is not valid.", data.Message);
            Assert.Equal(400, data.Code);
            Assert.False(data.Success);
        }

        [Fact]
        public async Task BadRequestRequestTest()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/badrequest/post", new BadRequestRequest(), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(data);
            Assert.Equal("Name不能为空", data.Message);
            Assert.Equal(400, data.Code);
            Assert.False(data.Success);
        }

        [Fact]
        public async Task PostTest()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
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
            await Task.Delay(3000); //等待事件处理完成
        }


        [Fact]
        public async Task QueryOrderByNameTest()
        {
            var client = factory.CreateClient();
            var response =
                await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na2", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var r = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(r);


            client = factory.CreateClient();
            response = await client.GetAsync("/query/orderbyname?name=na2");
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData<PagedData<GetOrderByNameDto>>>(JsonOption);
            Assert.NotNull(data);
            Assert.True(data.Success);
            Assert.Single(data.Data.Items);
            Assert.Equal("na2", data.Data.Items.First().Name);
        }


        [Fact]
        public async Task SetPaidTest()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);


            response = await client.GetAsync($"/get/{data.Id}");
            Assert.True(response.IsSuccessStatusCode);
            var queryResult = await response.Content.ReadFromJsonAsync<OrderQueryResult>(JsonOption);
            Assert.NotNull(queryResult);
            Assert.Equal("na", queryResult.Name);
            Assert.Equal(14, queryResult.Count);
            Assert.False(queryResult.Paid);
            Assert.Equal(0, queryResult.RowVersion.VersionNumber);

            response = await client.GetAsync($"/setPaid?id={data.Id}");
            Assert.True(response.IsSuccessStatusCode);
            var rd = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(rd);
            Assert.True(rd.Success);

            response = await client.GetAsync($"/get/{data.Id}");
            Assert.True(response.IsSuccessStatusCode);
            var queryResult2 = await response.Content.ReadFromJsonAsync<OrderQueryResult>(JsonOption);
            Assert.NotNull(queryResult2);
            Assert.Equal("na", queryResult2.Name);
            Assert.Equal(14, queryResult2.Count);
            Assert.True(queryResult2.Paid);
            Assert.Equal(1, queryResult2.RowVersion.VersionNumber);
            Assert.True(queryResult2.UpdateAt.Value > queryResult.UpdateAt.Value);
        }


        [Fact]
        public async Task SetOrderItemNameTest()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);

            response = await client.PostAsJsonAsync($"/setorderItemName?id={data.Id}&name=newName", new { },
                JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var rd = await response.Content.ReadFromJsonAsync<ResponseData>(JsonOption);
            Assert.NotNull(rd);

            response = await client.GetAsync($"/get/{data.Id}");
            Assert.True(response.IsSuccessStatusCode);
            var queryResult = await response.Content.ReadFromJsonAsync<OrderQueryResult>(JsonOption);
            Assert.NotNull(queryResult);
            Assert.Equal("newName", queryResult.Name);
            Assert.Equal(14, queryResult.Count);
            Assert.False(queryResult.Paid);
            Assert.Equal(1, queryResult.RowVersion.VersionNumber);
            Assert.Single(queryResult.OrderItems);
            Assert.Equal(1, queryResult.OrderItems.First().RowVersion.VersionNumber);
        }

        [Fact]
        public async Task Int64StronglyTypedId_FromRoute_Should_Work_Test()
        {
            int id = Random.Shared.Next();
            var client = factory.CreateClient();
            var response = await client.GetAsync($"/path/{id}");
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<ResponseData<OrderId>>(JsonOption);
            Assert.NotNull(data);
            Assert.True(data.Success);
            Assert.Equal(id, data.Data.Id);
        }

        [Fact]
        public async Task CreateOrderValidator_Should_ValidateWithAsync_WhenPosting()
        {
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);


            response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, -1), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var errData = await response.Content.ReadFromJsonAsync<ResponseData>();
            Assert.NotNull(errData);
            Assert.Contains("Count", errData.Message);
            Assert.Equal(400, errData.Code);
            Assert.False(errData.Success);
        }

        [Fact]
        public async Task ListOrdersByPage_Should_Work_Test()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);
            response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 60, 5), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);

            var orderName = "na";
            var countTotal = true;
            response = await client.GetAsync($"/list?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            var responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            var pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.True(pagedData.Total > 0);
            Assert.Single(pagedData.Items);

            countTotal = false;
            response = await client.GetAsync($"/list?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.Equal(0, pagedData.Total);
            Assert.Single(pagedData.Items);
        }

        [Fact]
        public async Task ListOrdersByPageSync_Should_Work_Test()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);
            response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 60, 5), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);

            var orderName = "na";
            var countTotal = true;
            response = await client.GetAsync(
                $"/listSync?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            var responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            var pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.True(pagedData.Total > 0);
            Assert.Single(pagedData.Items);

            countTotal = false;
            response = await client.GetAsync(
                $"/listSync?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.Equal(0, pagedData.Total);
            Assert.Single(pagedData.Items);
        }

        [Fact]
        public async Task ListOrdersPagedByPageRequest_Should_Work_Test()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);
            response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 60, 5), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);

            var orderName = "na";
            var countTotal = true;
            response = await client.GetAsync(
                $"/pagedList?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            var responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            var pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.True(pagedData.Total > 0);
            Assert.Single(pagedData.Items);

            countTotal = false;
            response = await client.GetAsync(
                $"/pagedList?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.Equal(0, pagedData.Total);
            Assert.Single(pagedData.Items);
        }

        [Fact]
        public async Task ListOrdersPagedByPageRequestAsync_Should_Work_Test()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);
            response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("na", 60, 5), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);

            var orderName = "na";
            var countTotal = true;
            response = await client.GetAsync(
                $"/pagedListAsync?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            var responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            var pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.True(pagedData.Total > 0);
            Assert.Single(pagedData.Items);

            countTotal = false;
            response = await client.GetAsync(
                $"/pagedListAsync?name={orderName}&pageIndex=2&pageSize=1&countTotal={countTotal}");
            Assert.True(response.IsSuccessStatusCode);
            responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.PageIndex);
            Assert.Equal(1, pagedData.PageSize);
            Assert.Equal(0, pagedData.Total);
            Assert.Single(pagedData.Items);
        }

        [Fact]
        public async Task JwtTokenTest()
        {
            var client = factory.CreateClient();

            var r = await client.PostAsJsonAsync("/jwtlogin", new { name = "test" }, JsonOption);
            
            Assert.True(r.IsSuccessStatusCode);
            var responseData = await r.Content.ReadFromJsonAsync<ResponseData<string>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.NotEmpty(responseData.Data);
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseData.Data);

            var r2 = await client.GetAsync("/jwt");
            Assert.True(r2.IsSuccessStatusCode);
        }

        [Fact]
        public async Task SoftDeleteTest()
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("sd", 55, 14), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            var data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);
            response = await client.PostAsJsonAsync("/api/order", new CreateOrderRequest("sd", 60, 5), JsonOption);
            Assert.True(response.IsSuccessStatusCode);
            data = await response.Content.ReadFromJsonAsync<OrderId>(JsonOption);
            Assert.NotNull(data);

            var orderName = "sd";
            response = await client.GetAsync(
                $"/pagedListAsync?name={orderName}&pageIndex=1&countTotal={true}");
            Assert.True(response.IsSuccessStatusCode);
            var responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            var pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(2, pagedData.Total);

            response = await client.DeleteAsync($"/delete/{data.Id}");
            Assert.True(response.IsSuccessStatusCode);

            response = await client.GetAsync(
                $"/pagedListAsync?name={orderName}&pageIndex=1&countTotal={true}");
            Assert.True(response.IsSuccessStatusCode);
            responseData =
                await response.Content.ReadFromJsonAsync<ResponseData<PagedData<OrderQueryResult>>>(JsonOption);
            Assert.NotNull(responseData);
            Assert.True(responseData.Success);
            pagedData = responseData.Data;
            Assert.NotNull(pagedData);
            Assert.Equal(1, pagedData.Total);

            var getDeletedItemResponse = await client.GetAsync($"/getIgnoreQueryFilter/{data.Id}");
            Assert.True(getDeletedItemResponse.IsSuccessStatusCode);
            var deleted = await getDeletedItemResponse.Content.ReadFromJsonAsync<OrderQueryResult>(JsonOption);
            Assert.NotNull(deleted);
            Assert.True(deleted.Deleted);
            Assert.True(deleted.DeletedTime.Value > DateTimeOffset.MinValue);
        }

        [Fact]
        public async Task OrderBy_Test()
        {
            using var scope = this.factory.Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            await db.Orders.AddAsync(new Order("a", 1));
            await db.Orders.AddAsync(new Order("b", 2));
            await db.Orders.AddAsync(new Order("c", 3));
            await db.SaveChangesAsync();
            
            using var scope2 = this.factory.Server.Services.CreateScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var orders = await db2.Orders.OrderBy(p => p.DeletedTime).ToListAsync();

            Assert.NotNull(orders);
        }


        [Fact]
        public async Task Int32StronglyTypedId_Compare_Should_Work_Test()
        {
            await using var scope = this.factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


            var orders = new List<Int32Order>();
            orders.Add(new("test1"));
            orders.Add(new("test2"));
            orders.Add(new("test3"));
            orders.Add(new("test4"));
            orders.Add(new("test5"));

            await db.Int32Orders.AddRangeAsync(orders);
            await db.SaveChangesAsync();

            await using var scope2 = this.factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var list = await db2.Int32Orders.Where(p => p.Id <= orders[2].Id).ToListAsync();
            Assert.Equal(3, list.Count);
            Assert.Equal(orders[0].Id, list[0].Id);
            Assert.Equal(orders[1].Id, list[1].Id);
            Assert.Equal(orders[2].Id, list[2].Id);

            list = await db2.Int32Orders.Where(p => p.Id < orders[2].Id).ToListAsync();
            Assert.Equal(2, list.Count);
            Assert.Equal(orders[0].Id, list[0].Id);
            Assert.Equal(orders[1].Id, list[1].Id);

            list = await db2.Int32Orders.Where(p => p.Id > orders[2].Id).ToListAsync();
            Assert.Equal(2, list.Count);
            Assert.Equal(orders[3].Id, list[0].Id);
            Assert.Equal(orders[4].Id, list[1].Id);

            list = await db2.Int32Orders.Where(p => p.Id >= orders[2].Id).ToListAsync();
            Assert.Equal(3, list.Count);
            Assert.Equal(orders[2].Id, list[0].Id);
            Assert.Equal(orders[3].Id, list[1].Id);
            Assert.Equal(orders[4].Id, list[2].Id);
        }

        [Fact]
        public async Task Int64StronglyTypedId_Compare_Should_Work_Test()
        {
            await using var scope = this.factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


            var orders = new List<Int64Order>();
            orders.Add(new("test1"));
            orders.Add(new("test2"));
            orders.Add(new("test3"));
            orders.Add(new("test4"));
            orders.Add(new("test5"));

            await db.Int64Orders.AddRangeAsync(orders);
            await db.SaveChangesAsync();

            await using var scope2 = this.factory.Services.CreateAsyncScope();
            var db2 = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var list = await db2.Int64Orders.Where(p => p.Id <= orders[2].Id).ToListAsync();
            Assert.Equal(3, list.Count);
            Assert.Equal(orders[0].Id, list[0].Id);
            Assert.Equal(orders[1].Id, list[1].Id);
            Assert.Equal(orders[2].Id, list[2].Id);

            list = await db2.Int64Orders.Where(p => p.Id < orders[2].Id).ToListAsync();
            Assert.Equal(2, list.Count);
            Assert.Equal(orders[0].Id, list[0].Id);
            Assert.Equal(orders[1].Id, list[1].Id);

            list = await db2.Int64Orders.Where(p => p.Id > orders[2].Id).ToListAsync();
            Assert.Equal(2, list.Count);
            Assert.Equal(orders[3].Id, list[0].Id);
            Assert.Equal(orders[4].Id, list[1].Id);

            list = await db2.Int64Orders.Where(p => p.Id >= orders[2].Id).ToListAsync();
            Assert.Equal(3, list.Count);
            Assert.Equal(orders[2].Id, list[0].Id);
            Assert.Equal(orders[3].Id, list[1].Id);
            Assert.Equal(orders[4].Id, list[2].Id);
        }
        
        [Fact]
        public async Task GuidStronglyTypedId_Compare_Should_Work_Test()
        {
            await using var scope = this.factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


            var orders = new List<GuidOrder>();
            orders.Add(new("test1"));
            orders.Add(new("test2"));
            orders.Add(new("test3"));
            orders.Add(new("test4"));
            orders.Add(new("test5"));

            await db.GuidOrders.AddRangeAsync(orders);
            await db.SaveChangesAsync();

            await using var scope2 = this.factory.Services.CreateAsyncScope();
            var db2 = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var list = await db2.GuidOrders.Where(p => p.Id <= orders[2].Id).ToListAsync();
            Assert.Equal(3, list.Count);
            Assert.Equal(orders[0].Id, list[0].Id);
            Assert.Equal(orders[1].Id, list[1].Id);
            Assert.Equal(orders[2].Id, list[2].Id);

            list = await db2.GuidOrders.Where(p => p.Id < orders[2].Id).ToListAsync();
            Assert.Equal(2, list.Count);
            Assert.Equal(orders[0].Id, list[0].Id);
            Assert.Equal(orders[1].Id, list[1].Id);

            list = await db2.GuidOrders.Where(p => p.Id > orders[2].Id).ToListAsync();
            Assert.Equal(2, list.Count);
            Assert.Equal(orders[3].Id, list[0].Id);
            Assert.Equal(orders[4].Id, list[1].Id);

            list = await db2.GuidOrders.Where(p => p.Id >= orders[2].Id).ToListAsync();
            Assert.Equal(3, list.Count);
            Assert.Equal(orders[2].Id, list[0].Id);
            Assert.Equal(orders[3].Id, list[1].Id);
            Assert.Equal(orders[4].Id, list[2].Id);
        }
    }
}