namespace NetCorePal.OpenTelemetry.Diagnostics;

/// <summary>
/// NetCorePalInstrumentation
/// </summary>
public class NetCorePalInstrumentation : IDisposable
{
    private readonly DiagnosticSourceSubscriber? _diagnosticSourceSubscriber;

    public NetCorePalInstrumentation(DiagnosticListener diagnosticListener)
    {
        _diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(diagnosticListener, null);
        _diagnosticSourceSubscriber.Subscribe();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _diagnosticSourceSubscriber?.Dispose();
    }
}