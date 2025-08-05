using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SkyApm;
using SkyApm.Config;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SkyApm.Common;
using System.Collections.Concurrent;

namespace NetCorePal.Web;

public class EntityFrameworkCoreConnectionTracingDiagnosticProcessor : ITracingDiagnosticProcessor
{
    private Func<ConnectionEventData, string>? _operationNameResolver;
    private Func<CommandEventData, string>? _commandOperationNameResolver;
    private readonly ITracingContext _tracingContext;
    private readonly TracingConfig _tracingConfig;
    private readonly bool _logParameterValue;
    private readonly ConcurrentDictionary<Guid, SegmentContext> _connectionOpenContexts = new();
    private readonly ConcurrentDictionary<Guid, SegmentContext> _connectionCloseContexts = new();
    private readonly ConcurrentDictionary<DbCommand, SegmentContext> _commandContexts = new();

    public string ListenerName => DbLoggerCategory.Name;

    /// <summary>
    /// A delegate that returns the OpenTracing "operation name" for the given connection event.
    /// </summary>
    public Func<ConnectionEventData, string> OperationNameResolver
    {
        get
        {
            return _operationNameResolver ??
                   (_operationNameResolver = (data) =>
                   {
                       return "DB Connection " + (data.Connection.Database ?? "Unknown");
                   });
        }
        set => _operationNameResolver = value ??
                                        throw new ArgumentNullException(nameof(OperationNameResolver));
    }

    /// <summary>
    /// A delegate that returns the OpenTracing "operation name" for the given command.
    /// </summary>
    public Func<CommandEventData, string> CommandOperationNameResolver
    {
        get
        {
            return _commandOperationNameResolver ??
                   (_commandOperationNameResolver = (data) =>
                   {
                       var commandType = data.Command.CommandText?.Split(' ');
                       return "DB " + (commandType?.FirstOrDefault() ?? data.ExecuteMethod.ToString());
                   });
        }
        set => _commandOperationNameResolver = value ??
                                               throw new ArgumentNullException(nameof(CommandOperationNameResolver));
    }

    public EntityFrameworkCoreConnectionTracingDiagnosticProcessor(
        ITracingContext tracingContext, IConfigAccessor configAccessor)
    {
        _tracingContext = tracingContext;
        _tracingConfig = configAccessor.Get<TracingConfig>();
        _logParameterValue = configAccessor.Get<SamplingConfig>().LogSqlParameterValue;
    }

    private string GetConnectionKey(DbConnection connection)
    {
        // 使用连接字符串和对象哈希码的组合作为唯一键
        return $"{connection.ConnectionString?.GetHashCode() ?? 0}_{connection.GetHashCode()}";
    }

    private string BuildParameterVariables(DbParameterCollection dbParameters)
    {
        if (dbParameters == null)
        {
            return string.Empty;
        }

        if (!_logParameterValue)
        {
            return string.Empty;
        }

        var parameters = new List<string>();
        foreach (DbParameter parameter in dbParameters)
        {
            parameters.Add($"{parameter.ParameterName}={parameter.Value ?? "NULL"}");
        }

        return string.Join(", ", parameters);
    }

    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Connection.ConnectionOpening")]
    public void ConnectionOpening([Object] ConnectionEventData eventData)
    {
        if (eventData?.Connection == null)
        {
            return;
        }

        var operationName = "DB Connection Open " + (eventData.Connection.Database ?? "Unknown");
        var context = _tracingContext.CreateLocalSegmentContext(operationName);

        context.Span.SpanLayer = SpanLayer.DB;
        context.Span.AddTag(Tags.DB_TYPE, "Sql");
        context.Span.AddTag(Tags.DB_INSTANCE, eventData.Connection.Database ?? "Unknown");
        context.Span.AddTag(Tags.DB_STATEMENT, "Connection Opening");
        //context.Span.AddTag("db.connection.string", eventData.Connection.ConnectionString ?? "Unknown");

        _connectionOpenContexts[eventData.ConnectionId] = context;
    }

    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Connection.ConnectionOpened")]
    public void ConnectionOpened([Object] ConnectionEventData eventData)
    {
        if (eventData?.Connection == null)
        {
            return;
        }

        var connectionKey = eventData.ConnectionId;
        if (_connectionOpenContexts.TryRemove(connectionKey, out var context))
        {
            context.Span.AddTag("db.connection.state", "opened");
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Connection.ConnectionClosing")]
    public void ConnectionClosing([Object] ConnectionEventData eventData)
    {
        if (eventData?.Connection == null)
        {
            return;
        }

        var operationName = "DB Connection Close " + (eventData.Connection.Database ?? "Unknown");
        var context = _tracingContext.CreateLocalSegmentContext(operationName);

        context.Span.SpanLayer = SpanLayer.DB;
        context.Span.AddTag(Tags.DB_TYPE, "Sql");
        context.Span.AddTag(Tags.DB_INSTANCE, eventData.Connection.Database ?? "Unknown");
        context.Span.AddTag(Tags.DB_STATEMENT, "Connection Closing");
        //context.Span.AddTag("db.connection.string", eventData.Connection.ConnectionString ?? "Unknown");

        _connectionCloseContexts[eventData.ConnectionId] = context;
    }

    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Connection.ConnectionClosed")]
    public void ConnectionClosed([Object] ConnectionEventData eventData)
    {
        if (eventData?.Connection == null)
        {
            return;
        }

        if (_connectionCloseContexts.TryRemove(eventData.ConnectionId, out var context))
        {
            context.Span.AddTag("db.connection.state", "closed");
            context.Span.AddLog(LogEvent.Event("ConnectionClosed"));

            _tracingContext.Release(context);
        }
    }

    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Connection.ConnectionError")]
    public void ConnectionError([Object] ConnectionErrorEventData eventData)
    {
        if (eventData?.Connection == null)
        {
            return;
        }

        if (_connectionOpenContexts.TryRemove(eventData.ConnectionId, out var context) ||
            _connectionCloseContexts.TryRemove(eventData.ConnectionId, out context))
        {
            context.Span.AddLog(LogEvent.Event("ConnectionError"));
            _tracingContext.Release(context);
        }
    }

    // 命令跟踪方法
    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
    public void CommandExecuting([Object] CommandEventData eventData)
    {
        if (eventData?.Command == null)
        {
            return;
        }

        var operationName = CommandOperationNameResolver(eventData);
        var context = _tracingContext.CreateLocalSegmentContext(operationName);

        context.Span.SpanLayer = SpanLayer.DB;
        context.Span.AddTag(Tags.DB_TYPE, "Sql");
        context.Span.AddTag(Tags.DB_INSTANCE, eventData.Command.Connection?.Database ?? "Unknown");
        context.Span.AddTag(Tags.DB_STATEMENT, eventData.Command.CommandText ?? "Unknown");
        context.Span.AddTag(Tags.DB_BIND_VARIABLES, BuildParameterVariables(eventData.Command.Parameters));

        _commandContexts[eventData.Command] = context;
    }

    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
    public void CommandExecuted([Object] CommandExecutedEventData eventData)
    {
        if (eventData?.Command == null)
        {
            return;
        }

        if (_commandContexts.TryRemove(eventData.Command, out var context))
        {
            context.Span.AddLog(LogEvent.Event("CommandExecuted"));
            _tracingContext.Release(context);
        }
    }

    [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
    public void CommandError([Object] CommandErrorEventData eventData)
    {
        if (eventData?.Command == null)
        {
            return;
        }

        if (_commandContexts.TryRemove(eventData.Command, out var context))
        {
            context.Span.ErrorOccurred(eventData.Exception, _tracingConfig);
            context.Span.AddLog(LogEvent.Event("CommandError"));
            _tracingContext.Release(context);
        }
    }
}