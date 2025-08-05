using System.Collections.Concurrent;
using SkyApm;
using NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;

namespace NetCorePal.SkyApm.Diagnostics;

public class RedisLockTracingDiagnosticProcessor : ITracingDiagnosticProcessor
{
    public string ListenerName => NetCorePalRedisDiagnosticListenerNames.DiagnosticListenerName;

    private readonly ConcurrentDictionary<Guid, SegmentContext> _AcquireContexts = new();
    private readonly ConcurrentDictionary<Guid, SegmentContext> _ReleaseContexts = new();

    private readonly ITracingContext _tracingContext;
    private readonly TracingConfig _tracingConfig;

    public RedisLockTracingDiagnosticProcessor(ITracingContext tracingContext, IConfigAccessor configAccessor)
    {
        _tracingContext = tracingContext;
        _tracingConfig = configAccessor.Get<TracingConfig>();
    }


    [DiagnosticName(NetCorePalRedisDiagnosticListenerNames.AcquireBegin)]
    public void AcquireBegin([Object] AcquireBeginData eventData)
    {
        var operationName = $"RedisLock {eventData.MethodName}";
        var context = _tracingContext.CreateLocalSegmentContext(operationName);
        context.Span.SpanLayer = SpanLayer.DB;
        context.Span.AddTag(Tags.DB_TYPE, "Redis");
        _AcquireContexts[eventData.Id] = context;
    }

    [DiagnosticName(NetCorePalRedisDiagnosticListenerNames.AcquireEnd)]
    public void AcquireEnd([Object] AcquireEndData eventData)
    {
        if (_AcquireContexts.TryRemove(eventData.Id, out var context))
        {
            context.Span.AddTag("RedisLock", "AcquireEnd");
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalRedisDiagnosticListenerNames.AcquireError)]
    public void AcquireError([Object] AcquireErrorData eventData)
    {
        if (_AcquireContexts.TryRemove(eventData.Id, out var context))
        {
            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            context.Span.AddTag("RedisLock", "AcquireError");
        }
    }

    [DiagnosticName(NetCorePalRedisDiagnosticListenerNames.ReleaseBegin)]
    public void ReleaseBegin([Object] ReleaseBeginData eventData)
    {
        var operationName = $"RedisLock Release {eventData.MethodName}";
        var context = _tracingContext.CreateLocalSegmentContext(operationName);
        context.Span.SpanLayer = SpanLayer.DB;
        context.Span.AddTag(Tags.DB_TYPE, "Redis");
        _ReleaseContexts[eventData.Id] = context;
    }

    [DiagnosticName(NetCorePalRedisDiagnosticListenerNames.ReleaseEnd)]
    public void ReleaseEnd([Object] ReleaseEndData eventData)
    {
        if (_ReleaseContexts.TryRemove(eventData.Id, out var context))
        {
            context.Span.AddTag("RedisLock", "ReleaseEnd");
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName(NetCorePalRedisDiagnosticListenerNames.ReleaseError)]
    public void ReleaseError([Object] ReleaseErrorData eventData)
    {
        if (_ReleaseContexts.TryRemove(eventData.Id, out var context))
        {
            context.Span.AddTag("RedisLock", "ReleaseError");
        }
    }
}