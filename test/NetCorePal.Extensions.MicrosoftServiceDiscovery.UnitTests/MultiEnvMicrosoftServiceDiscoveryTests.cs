using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
using NetCorePal.Context;
using NetCorePal.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.MicrosoftServiceDiscovery.UnitTests;

public class MultiEnvMicrosoftServiceDiscoveryTests
{
    private IServiceCollection Setup(IServiceCollection services)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder();
        builder.AddJsonFile("appsettings.json");
        IConfigurationRoot configuration = builder.Build();
        services.AddContextCore().AddEnvContext();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMultiEnv(options => { options.ServiceName = "abc"; })
            .UseMicrosoftServiceDiscovery();
        services.AddConfigurationServiceEndpointProvider();
        return services;
    }

    [Fact]
    public async Task Use_Default_Service_When_EnvContext_Is_Null()
    {
        IServiceCollection services = new ServiceCollection();
        Setup(services);
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        services.ConfigureHttpClientDefaults(b =>
        {
            b.AddMultiEnvMicrosoftServiceDiscovery();
            b.AddHttpMessageHandler(() => mockHttpMessageHandler);
        });
        var provider = services.BuildServiceProvider();

        var cli = provider.GetRequiredService<IHttpClientFactory>();
        var s = cli.CreateClient("a");
        s.BaseAddress = new Uri("https+http://order:88");
        var r = await s.GetAsync("/abc");
        Assert.NotNull(mockHttpMessageHandler.RequestUri);
        Assert.Equal("https://order:8443/abc", mockHttpMessageHandler.RequestUri.AbsoluteUri);
        
        var r2 = await s.GetAsync("/abc2");
    }

    [Fact]
    public async Task Use_Default_Service_When_EnvContext_Is_Default()
    {
        IServiceCollection services = new ServiceCollection();
        Setup(services);
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        services.ConfigureHttpClientDefaults(b =>
        {
            b.AddMultiEnvMicrosoftServiceDiscovery();
            b.AddHttpMessageHandler(() => mockHttpMessageHandler);
        });
        var provider = services.BuildServiceProvider();

        var contextAccessor = provider.GetRequiredService<IContextAccessor>();
        contextAccessor.SetContext(new EnvContext("main"));

        var cli = provider.GetRequiredService<IHttpClientFactory>();
        var s = cli.CreateClient("a");
        s.BaseAddress = new Uri("https+http://order:88");
        var r = await s.GetAsync("/abc");
        Assert.NotNull(mockHttpMessageHandler.RequestUri);
        Assert.Equal("https://order:8443/abc", mockHttpMessageHandler.RequestUri.AbsoluteUri);
    }

    [Fact]
    public async Task Use_Default_Service_When_EnvContext_Is_V2()
    {
        IServiceCollection services = new ServiceCollection();
        Setup(services);
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        services.ConfigureHttpClientDefaults(b =>
        {
            b.AddMultiEnvMicrosoftServiceDiscovery();
            b.AddHttpMessageHandler(() => mockHttpMessageHandler);
        });
        var provider = services.BuildServiceProvider();

        var contextAccessor = provider.GetRequiredService<IContextAccessor>();
        contextAccessor.SetContext(new EnvContext("v2"));

        var cli = provider.GetRequiredService<IHttpClientFactory>();
        var s = cli.CreateClient("a");
        s.BaseAddress = new Uri("https+http://order:88");
        var r = await s.GetAsync("/abc");
        Assert.NotNull(mockHttpMessageHandler.RequestUri);
        Assert.Equal("https://order-v2:8443/abc", mockHttpMessageHandler.RequestUri.AbsoluteUri);
    }

    [Fact]
    public async Task Use_Default_Service_When_EnvContext_Is_Not_Exist()
    {
        IServiceCollection services = new ServiceCollection();
        Setup(services);
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        services.ConfigureHttpClientDefaults(b =>
        {
            b.AddMultiEnvMicrosoftServiceDiscovery();
            b.AddHttpMessageHandler(() => mockHttpMessageHandler);
        });
        var provider = services.BuildServiceProvider();

        var contextAccessor = provider.GetRequiredService<IContextAccessor>();
        contextAccessor.SetContext(new EnvContext("v3"));

        var cli = provider.GetRequiredService<IHttpClientFactory>();
        var s = cli.CreateClient("a");
        s.BaseAddress = new Uri("https+http://order:88");
        var r = await s.GetAsync("/abc");
        Assert.NotNull(mockHttpMessageHandler.RequestUri);
        Assert.Equal("https://order:8443/abc", mockHttpMessageHandler.RequestUri.AbsoluteUri);
    }
}

class MockHttpMessageHandler : DelegatingHandler
{
    public MockHttpMessageHandler()
    {
    }

    public Uri? RequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        this.RequestUri = request.RequestUri;
        return Task.FromResult(new HttpResponseMessage());
    }
}