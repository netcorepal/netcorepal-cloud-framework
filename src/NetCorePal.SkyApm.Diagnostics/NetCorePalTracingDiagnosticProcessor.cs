using System.Collections.Concurrent;
using NetCorePal.Extensions.Primitives.Diagnostics;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace NetCorePal.SkyApm.Diagnostics;

public class NetCorePalTracingDiagnosticProcessor : ITracingDiagnosticProcessor
{
    private readonly ConcurrentDictionary<Guid, SegmentContext> _commandContexts =
        new ConcurrentDictionary<Guid, SegmentContext>();

    private StringOrIntValue _component = new StringOrIntValue(3399, "NetCorePal");

    private readonly ConcurrentDictionary<Guid, SegmentContext> _transactionContexts =
        new ConcurrentDictionary<Guid, SegmentContext>();

    private readonly ConcurrentDictionary<Guid, SegmentContext> _domainEventHandlerContexts =
        new ConcurrentDictionary<Guid, SegmentContext>();

    private readonly ConcurrentDictionary<Guid, SegmentContext> _saveChangesContexts =
        new ConcurrentDictionary<Guid, SegmentContext>();

    public string ListenerName => NetCorePalDiagnosticListenerNames.DiagnosticListenerName;

    private readonly ITracingContext _tracingContext;
    private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
    private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;
    private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
    private readonly TracingConfig _tracingConfig;

    public NetCorePalTracingDiagnosticProcessor(ITracingContext tracingContext,
        IEntrySegmentContextAccessor entrySegmentContextAccessor,
        IExitSegmentContextAccessor exitSegmentContextAccessor,
        ILocalSegmentContextAccessor localSegmentContextAccessor,
        IConfigAccessor configAccessor)
    {
        _tracingContext = tracingContext;
        _exitSegmentContextAccessor = exitSegmentContextAccessor;
        _localSegmentContextAccessor = localSegmentContextAccessor;
        _entrySegmentContextAccessor = entrySegmentContextAccessor;
        _tracingConfig = configAccessor.Get<TracingConfig>();
    }


    [DiagnosticName(NetCorePalDiagnosticListenerNames.CommandHandlerBegin)]
    public void CommandBegin([Object] CommandBegin eventData)
    {
        var context = _tracingContext.CreateLocalSegmentContext(eventData.Name);
        context.Span.SpanLayer = SpanLayer.DB;
        context.Span.Component = _component;
        context.Span.AddTag("CommandName", eventData.Name);
        context.Span.AddLog(LogEvent.Event("CommandBegin"));
        _commandContexts[eventData.Id] = context;
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.CommandHandlerEnd)]
    public void CommandEnd([Object] CommandEnd eventData)
    {
        var context = _commandContexts[eventData.Id];
        context.Span.AddLog(LogEvent.Event("CommandEnd"));
        context.Span.AddLog(LogEvent.Message("CommandEnd"));
        _tracingContext.Release(context);
        _commandContexts.TryRemove(eventData.Id, out _);
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.CommandHandlerError)]
    public void CommandError([Object] CommandError eventData)
    {
        var context = _commandContexts[eventData.Id];
        context.Span.AddLog(LogEvent.Event("CommandError"));
        context.Span.AddLog(LogEvent.Message("CommandError"));
        context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
        _tracingContext.Release(context);
        _commandContexts.TryRemove(eventData.Id, out _);
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.DomainEventHandlerBegin)]
    public void DomainEventHandlerBegin([Object] DomainEventHandlerBegin eventData)
    {
        var context =
            _tracingContext.CreateLocalSegmentContext(eventData.Name);
        context.Span.Component = _component;
        context.Span.AddLog(LogEvent.Event("DomainEventHandlerBegin"));
        context.Span.AddLog(LogEvent.Message("DomainEventHandlerBegin: " + eventData.Name));
        _domainEventHandlerContexts[eventData.Id] = context;
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.DomainEventHandlerEnd)]
    public void DomainEventHandlerEnd([Object] DomainEventHandlerEnd eventData)
    {
        var context = _domainEventHandlerContexts[eventData.Id];
        context.Span.AddLog(LogEvent.Event("DomainEventHandlerEnd"));
        context.Span.AddLog(LogEvent.Message("DomainEventHandlerEnd: " + eventData.Name));
        _tracingContext.Release(context);
        _domainEventHandlerContexts.TryRemove(eventData.Id, out _);
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.DomainEventHandlerError)]
    public void DomainEventHandlerError([Object] DomainEventHandlerError eventData)
    {
        var context = _domainEventHandlerContexts[eventData.Id];
        context.Span.AddLog(LogEvent.Event("DomainEventHandlerError"));
        context.Span.AddLog(LogEvent.Message("DomainEventHandlerError: " + eventData.Name));
        context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
        _tracingContext.Release(context);
        _domainEventHandlerContexts.TryRemove(eventData.Id, out _);
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.TransactionBegin)]
    public void TransactionBegin([Object] TransactionBegin eventData)
    {
        var context =
            _tracingContext.CreateLocalSegmentContext("TransactionBegin");
        context.Span.Component = _component;
        context.Span.AddLog(LogEvent.Event("TransactionBegin"));
        context.Span.AddLog(LogEvent.Message("TransactionBegin: " + eventData.TransactionId));
        _transactionContexts[eventData.TransactionId] = context;
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.TransactionCommit)]
    public void TransactionCommit([Object] TransactionCommit eventData)
    {
        var context = _transactionContexts[eventData.TransactionId];
        context.Span.AddLog(LogEvent.Event("TransactionCommit"));
        context.Span.AddLog(LogEvent.Message("TransactionCommit: " + eventData.TransactionId));
        _tracingContext.Release(context);
        _transactionContexts.TryRemove(eventData.TransactionId, out _);
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.TransactionRollback)]
    public void TransactionRollback([Object] TransactionRollback eventData)
    {
        var context = _transactionContexts[eventData.TransactionId];
        context.Span.AddLog(LogEvent.Event("TransactionRollback"));
        context.Span.AddLog(LogEvent.Message("TransactionRollback: " + eventData.TransactionId));
        _tracingContext.Release(context);
        _transactionContexts.TryRemove(eventData.TransactionId, out _);
    }
}