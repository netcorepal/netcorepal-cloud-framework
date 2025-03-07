using NetCorePal.Context;
using NetCorePal.Extensions.MultiEnv;

namespace Microsoft.Extensions.DependencyInjection;

public static class IHttpClientBuilderExtensions
{
    /// <summary>
    /// 注入多环境服务选择器以及Microsoft.Extensions.ServiceDiscovery;
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddMultiEnvMicrosoftServiceDiscovery(this IHttpClientBuilder builder)
    {
        builder.AddHttpMessageHandler(services =>
        {
            var contextAccessor = services.GetRequiredService<IContextAccessor>();
            var serviceSelector = services.GetRequiredService<IServiceChecker>();
            return new MultiEnvServiceDiscoveryHttpMessageHandler(contextAccessor, serviceSelector);
        });

        builder.AddServiceDiscovery();
        return builder;
    }
}

internal class MultiEnvServiceDiscoveryHttpMessageHandler(
    IContextAccessor contextAccessor,
    IServiceChecker serviceChecker) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri != null)
        {
            var envContext = contextAccessor.GetContext<EnvContext>();
            if (envContext != null)
            {
                var serviceName = request.RequestUri.GetLeftPart(UriPartial.Authority);

                var envService = serviceChecker.GetEnvServiceName(serviceName, envContext.Env);

                if (await serviceChecker.ServiceExist(envService))
                {
                    request.RequestUri = new Uri(request.RequestUri.AbsoluteUri.Replace(serviceName, envService));
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}