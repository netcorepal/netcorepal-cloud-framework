using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Context.Diagnostics.HttpClient;
using NetCorePal.Extensions.DependencyInjection;

namespace NetCorePal.Context.AspNetCore.UnitTests;

public class HttpClientContextCarrierTests
{
    [Fact]
    public void HttpClientContextCarrier_Tests()
    {
        var services = new ServiceCollection();
        services.AddContext().AddEnvContext();
        var serviceProvider = services.BuildServiceProvider();
        var contextAccessor = serviceProvider.GetRequiredService<IContextAccessor>();
        var processor = serviceProvider.GetRequiredService<HttpClientDiagnosticContextProcessor>();
        var request = new HttpRequestMessage();
        processor.HttpRequest(request);
        var contextCarrier = new HttpClientContextCarrier(request);

        contextCarrier.Set("test", "v1");
        Assert.True(request.Headers.Contains("test"));
        Assert.Equal("v1", request.Headers.GetValues("test").First());
    }
}