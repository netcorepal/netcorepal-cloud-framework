using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.AspNetCore.HttpClients;

public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Add KnownExceptionDelegatingHandler to HttpClientBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddKnownExceptionDelegatingHandler(this IHttpClientBuilder builder)
    {
        builder.Services.AddTransient<KnownExceptionDelegatingHandler>();
        builder.AddHttpMessageHandler<KnownExceptionDelegatingHandler>();
        return builder;
    }
}