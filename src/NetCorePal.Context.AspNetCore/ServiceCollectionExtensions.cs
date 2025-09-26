using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Context;
using NetCorePal.Context.Diagnostics.AspNetCore;
using NetCorePal.Context.Diagnostics.HttpClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContext(this IServiceCollection services)
        {
            services.AddContextCore();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IContextProcessor, AspNetCoreHostingDiagnosticContextProcessor>());
            services.TryAddSingleton<AspNetCoreHostingDiagnosticContextProcessor>(p => (p.GetRequiredService<IEnumerable<IContextProcessor>>().FirstOrDefault(cp => cp is AspNetCoreHostingDiagnosticContextProcessor) as AspNetCoreHostingDiagnosticContextProcessor)!);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IContextProcessor, HttpClientDiagnosticContextProcessor>());
            services.TryAddSingleton<HttpClientDiagnosticContextProcessor>(p => (p.GetServices<IContextProcessor>().FirstOrDefault(cp => cp is HttpClientDiagnosticContextProcessor) as HttpClientDiagnosticContextProcessor)!);
            return services;
        }
    }
}
