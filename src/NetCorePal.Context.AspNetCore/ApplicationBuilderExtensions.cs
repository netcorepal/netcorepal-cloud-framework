using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Context;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseContext(this IApplicationBuilder app)
        {
            _ = app.ApplicationServices.GetServices<IContextProcessor>().ToList();
            return app;
        }
    }
}
