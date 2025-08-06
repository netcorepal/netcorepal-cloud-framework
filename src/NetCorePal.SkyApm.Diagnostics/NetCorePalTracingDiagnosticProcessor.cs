using System.Collections.Concurrent;
using NetCorePal.Extensions.Primitives.Diagnostics;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NetCorePal.SkyApm.Diagnostics;

public class NetCorePalTracingDiagnosticProcessor : ITracingDiagnosticProcessor
{
    private StringOrIntValue _component = new StringOrIntValue(3020, "NetCorePal");
    public string ListenerName => NetCorePalDiagnosticListenerNames.DiagnosticListenerName;

    private readonly ITracingContext _tracingContext;
    private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
    private readonly TracingConfig _tracingConfig;
    private readonly NetCorePalTracingOptions _options;

    public NetCorePalTracingDiagnosticProcessor(ITracingContext tracingContext,
        ILocalSegmentContextAccessor segmentContextAccessor,
        IConfigAccessor configAccessor,
        IOptions<NetCorePalTracingOptions> options)
    {
        _tracingContext = tracingContext;
        _localSegmentContextAccessor = segmentContextAccessor;
        _tracingConfig = configAccessor.Get<TracingConfig>();
        _options = options.Value;
    }


    [DiagnosticName(NetCorePalDiagnosticListenerNames.CommandHandlerBegin)]
    public void CommandBegin([Object] CommandBegin eventData)
    {
        var context = _tracingContext.CreateLocalSegmentContext(eventData.Name);
        context.Span.Component = _component;
        context.Span.AddTag("CommandName", eventData.Name);
        context.Span.AddLog(LogEvent.Event("CommandBegin"));
        if (_options.WriteCommandData)
        {
            context.Span.AddLog(LogEvent.Message("Command：" +
                                                 JsonSerializer.Serialize(eventData.CommandData,
                                                     _options.JsonSerializerOptions)));
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.CommandHandlerEnd)]
    public void CommandEnd([Object] CommandEnd eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("CommandEnd"));
            context.Span.AddLog(LogEvent.Message("CommandEnd"));
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.CommandHandlerError)]
    public void CommandError([Object] CommandError eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("CommandError"));
            context.Span.AddLog(LogEvent.Message("CommandError"));
            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.DomainEventHandlerBegin)]
    public void DomainEventHandlerBegin([Object] DomainEventHandlerBegin eventData)
    {
        var context =
            _tracingContext.CreateLocalSegmentContext(eventData.Name);
        context.Span.Component = _component;
        context.Span.AddLog(LogEvent.Event("DomainEventHandlerBegin"));
        context.Span.AddLog(LogEvent.Message("DomainEventHandlerBegin: " + eventData.Name));
        if (_options.WriteDomainEventData)
        {
            context.Span.AddLog(LogEvent.Message("DomainEventData：" +
                                                 JsonSerializer.Serialize(eventData.EventData,
                                                     _options.JsonSerializerOptions)));
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.DomainEventHandlerEnd)]
    public void DomainEventHandlerEnd([Object] DomainEventHandlerEnd eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("DomainEventHandlerEnd"));
            context.Span.AddLog(LogEvent.Message("DomainEventHandlerEnd: " + eventData.Name));
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.DomainEventHandlerError)]
    public void DomainEventHandlerError([Object] DomainEventHandlerError eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("DomainEventHandlerError"));
            context.Span.AddLog(LogEvent.Message("DomainEventHandlerError: " + eventData.Name));
            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.TransactionBegin)]
    public void TransactionBegin([Object] TransactionBegin eventData)
    {
        var context = _tracingContext.CreateLocalSegmentContext("Transaction");
        context.Span.Component = _component;
        context.Span.AddLog(LogEvent.Event("TransactionBegin"));
        context.Span.AddLog(LogEvent.Message("TransactionBegin: " + eventData.TransactionId));
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.TransactionCommit)]
    public void TransactionCommit([Object] TransactionCommit eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("TransactionCommit"));
            context.Span.AddLog(LogEvent.Message("TransactionCommit: " + eventData.TransactionId));
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.TransactionRollback)]
    public void TransactionRollback([Object] TransactionRollback eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("TransactionRollback"));
            context.Span.AddLog(LogEvent.Message("TransactionRollback: " + eventData.TransactionId));
            _tracingContext.Release(context);
        }
    }


    [DiagnosticName(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerBegin)]
    public void IntegrationEventHandlerBegin([Object] IntegrationEventHandlerBegin eventData)
    {
        var context =
            _tracingContext.CreateLocalSegmentContext(eventData.HandlerName);
        context.Span.Component = _component;
        context.Span.AddLog(LogEvent.Event("IntegrationEventHandlerBegin"));
        if (_options.WriteIntegrationEventData)
        {
            context.Span.AddLog(LogEvent.Message("IntegrationEventData：" +
                                                 JsonSerializer.Serialize(eventData.EventData,
                                                     _options.JsonSerializerOptions)));
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerEnd)]
    public void IntegrationEventHandlerEnd([Object] IntegrationEventHandlerEnd eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("IntegrationEventHandlerEnd"));
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalDiagnosticListenerNames.IntegrationEventHandlerError)]
    public void IntegrationEventHandlerError([Object] IntegrationEventHandlerError eventData)
    {
        var context = _localSegmentContextAccessor.Context;
        if (context != null)
        {
            context.Span.AddLog(LogEvent.Event("IntegrationEventHandlerError"));
            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            _tracingContext.Release(context);
        }
    }
}