using Microsoft.Extensions.DependencyInjection;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;

namespace NetCorePal.SkyApm.Diagnostics;

public static class SkyWalkingBuilderExtensions
{
    public static SkyApmExtensions AddNetCorePal(this SkyApmExtensions extensions)
    {
        if (extensions == null)
        {
            throw new ArgumentNullException(nameof(extensions));
        }

        extensions.Services.AddSingleton<ITracingDiagnosticProcessor, NetCorePalTracingDiagnosticProcessor>();

        return extensions;
    }
}