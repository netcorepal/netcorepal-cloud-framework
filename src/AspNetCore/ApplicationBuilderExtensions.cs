using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetCorePal.Extensions.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseKnownExceptionHandler(this IApplicationBuilder app, Action<KnownExceptionHandleMiddlewareOptions>? configOptions = null)
        {
            KnownExceptionHandleMiddlewareOptions options = new();
            configOptions?.Invoke(options);

            app.UseExceptionHandler(builder =>
            {
                builder.UseMiddleware<KnownExceptionHandleMiddleware>(options, app.ApplicationServices.GetRequiredService<ILogger<KnownExceptionHandleMiddleware>>());
            });
            return app;
        }
    }
}
