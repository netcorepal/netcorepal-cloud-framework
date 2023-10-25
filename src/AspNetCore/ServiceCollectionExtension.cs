using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.AspNetCore;

namespace NetCorePal.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRequestCancellationToken(this IServiceCollection services)
    {
        services.AddSingleton<IRequestCancellationToken, HttpContextAccessorRequestAbortedHandler>();
        return services;
    }
}