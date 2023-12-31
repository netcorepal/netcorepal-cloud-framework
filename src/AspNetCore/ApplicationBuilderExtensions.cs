using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetCorePal.Extensions.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseKnownExceptionHandler(this IApplicationBuilder app,
            Action<KnownExceptionHandleMiddlewareOptions>? configOptions = null)
        {
            KnownExceptionHandleMiddlewareOptions options = new();
            configOptions?.Invoke(options);

            app.UseExceptionHandler(builder =>
            {
                builder.UseMiddleware<KnownExceptionHandleMiddleware>(options,
                    app.ApplicationServices.GetRequiredService<ILogger<KnownExceptionHandleMiddleware>>());
            });
            return app;
        }


        /// <summary>
        /// 支持为不同的请求定义不同的配置
        /// </summary>
        /// <param name="app"></param>
        /// <param name="optionFactory"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseKnownExceptionHandler(this IApplicationBuilder app,
            Func<HttpContext, KnownExceptionHandleMiddlewareOptions> optionFactory)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.UseMiddleware<KnownExceptionHandleMiddleware>(optionFactory,
                    app.ApplicationServices.GetRequiredService<ILogger<KnownExceptionHandleMiddleware>>());
            });
            return app;
        }
    }
}