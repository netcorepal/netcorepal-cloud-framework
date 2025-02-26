using NetCorePal.Extensions.ServiceDiscovery;

namespace Microsoft.Extensions.DependencyInjection;

public static class IHttpClientBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddNetCorePalServiceDiscovery(this IHttpClientBuilder builder, string serviceName)
    {
        return builder.ConfigureHttpClient((serviceProvider, httpClient) =>
        {
            var serviceSelector = serviceProvider.GetRequiredService<IServiceSelector>();
            var service = serviceSelector.Find(serviceName);
            ArgumentNullException.ThrowIfNull(service);
            httpClient.BaseAddress = new Uri(service.Address);
        });
    }
}