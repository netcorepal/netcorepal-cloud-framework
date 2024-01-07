using Microsoft.Extensions.DependencyInjection;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;

namespace NetCorePal.SkyApm.Diagnostics;

public static class SkyWalkingBuilderExtensions
{
    public static SkyApmExtensions AddNetCorePal(this SkyApmExtensions extensions,
        Action<NetCorePalTracingOptions>? configAction = null)
    {
        if (extensions == null)
        {
            throw new ArgumentNullException(nameof(extensions));
        }
        extensions.Services.Configure<NetCorePalTracingOptions>(p => configAction?.Invoke(p));
        extensions.Services.AddSingleton<ITracingDiagnosticProcessor, NetCorePalTracingDiagnosticProcessor>();

        return extensions;
    }
}