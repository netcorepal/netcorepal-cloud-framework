using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NetCorePal.Extensions.AspNetCore.HttpClients;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class KnownExceptionDelegatingHandlerTests
{
    [Fact]
    public async Task SendAsync_Should_Throw_KnownException_When_Response_Is_Not_SuccessStatusCode()
    {
        Mock<DelegatingHandler> mockHttpMessageHandler = new();

        var data = new ResponseData(false, "error", code: 1,
            errorData: new object[] { new { a = 1, b = 2 }, new { a = 3, b = 4 } });
        var json = JsonSerializer.Serialize(data);
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(json)
            });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient("test")
            .AddKnownExceptionDelegatingHandler()
            .AddHttpMessageHandler(() => mockHttpMessageHandler.Object);
        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient("test");
        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost"), new CancellationToken()));

        Assert.Equal("error", ex.Message);
        Assert.Equal(1, ex.ErrorCode);
        Assert.Equal(2, ex.ErrorData.Count());
    }
    
    [Fact]
    public async Task SendAsync_Should_Not_Throw_KnownException_When_HttpRequestException_Occurs()
    {
        Mock<DelegatingHandler> mockHttpMessageHandler = new();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("HttpRequestException"));

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient("test")
            .AddKnownExceptionDelegatingHandler()
            .AddHttpMessageHandler(() => mockHttpMessageHandler.Object);
        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient("test");
        var ex = await Assert.ThrowsAsync<KnownException>(() =>
            client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost"), new CancellationToken()));

        Assert.Equal("未知错误", ex.Message);
    }
    

    

    [Fact]
    public async Task SendAsync_Should_Not_Throw_KnownException_When_NO_Exception_Occurs()
    {
        Mock<DelegatingHandler> mockHttpMessageHandler = new();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient("test")
            .AddKnownExceptionDelegatingHandler()
            .AddHttpMessageHandler(() => mockHttpMessageHandler.Object);
        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient("test");
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost"),
            new CancellationToken());
    }
}