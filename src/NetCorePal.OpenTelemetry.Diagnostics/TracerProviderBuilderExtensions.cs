using OpenTelemetry.Trace;

namespace NetCorePal.OpenTelemetry.Diagnostics;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddNetCorePalInstrumentation(this TracerProviderBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.AddSource(DiagnosticListener.SourceName);

        var instrumentation = new NetCorePalInstrumentation(new DiagnosticListener());

        return builder.AddInstrumentation(() => instrumentation);
    }

}