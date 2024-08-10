using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.AspNetCore.HttpClients;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class HttpClientBuilderExtensionsTests
{
    [Fact]
    public async Task AddKnownExceptionDelegatingHandlerTest()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient("test")
            .AddKnownExceptionDelegatingHandler();
        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("test");

        // Act
        var ex = await Assert.ThrowsAsync<KnownException>(() => httpClient.GetAsync("http://localhost/500"));

        // Assert
        Assert.Equal("未知错误", ex.Message);
    }
}