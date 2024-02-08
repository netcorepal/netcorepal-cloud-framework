using System.Diagnostics;
using NetCorePal.Extensions.Primitives.Diagnostics;

namespace NetCorePal.Extensions.DistributedTransactions;

public sealed class IntegrationEventHandlerWrap<TIntegrationEventHandler, TIntegrationEvent>
    where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : notnull
{
    private readonly IntegrationEventHandlerDelegate _next;

    public IntegrationEventHandlerWrap(TIntegrationEventHandler handler,
        IEnumerable<IIntegrationEventHandlerFilter> filters)
    {
        IntegrationEventHandlerDelegate next = async context =>
        {
            Guid id = Guid.NewGuid();
            var handlerName = typeof(TIntegrationEventHandler).FullName ?? typeof(TIntegrationEventHandler).Name;
            try
            {
                WriteIntegrationEventHandlerBegin(new IntegrationEventHandlerBegin(id, handlerName, context.Data));
                await handler.HandleAsync((TIntegrationEvent)context.Data, context.CancellationToken);
                WriteIntegrationEventHandlerEnd(new IntegrationEventHandlerEnd(id, handlerName, context.Data));
            }
            catch (Exception e)
            {
                WriteIntegrationEventHandlerError(new IntegrationEventHandlerError(id, handlerName, context.Data, e));
                throw;
            }
        };
        foreach (var filter in filters.Reverse())
        {
            var current = next;
            next = context => filter.HandleAsync(context, current);
        }

        _next = next;
    }


    public Task HandleAsync(TIntegrationEvent eventData,
        IDictionary<string, string?>? headers = default,
        CancellationToken cancellationToken = default)
    {
        var context =
            new IntegrationEventHandlerContext<TIntegrationEvent>(eventData,
                headers ?? new Dictionary<string, string?>(),
                cancellationToken);
        return _next.Invoke(context);
    }


    private static void WriteIntegrationEventHandlerBegin(IntegrationEventHandlerBegin eventData)
    {
        if (NetCorePalDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerBegin))
        {
            NetCorePalDiagnosticListenerNames.DiagnosticListener.Write(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerBegin, eventData);
        }
    }

    private static void WriteIntegrationEventHandlerEnd(IntegrationEventHandlerEnd eventData)
    {
        if (NetCorePalDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerEnd))
        {
            NetCorePalDiagnosticListenerNames.DiagnosticListener.Write(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerEnd, eventData);
        }
    }

    private static void WriteIntegrationEventHandlerError(IntegrationEventHandlerError eventData)
    {
        if (NetCorePalDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerError))
        {
            NetCorePalDiagnosticListenerNames.DiagnosticListener.Write(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerError, eventData);
        }
    }
}