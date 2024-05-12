using Hangfire;
using NetCorePal.Extensions.Hangfire;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用DependencyInjection容器作为JobActivator
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseHangfireContainerJobActivator(this IApplicationBuilder app)
    {
        var jobActivator = new ContainerJobActivator(app.ApplicationServices);
        GlobalConfiguration.Configuration.UseActivator(jobActivator);
        return app;
    }
}