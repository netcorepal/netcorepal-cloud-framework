using System.Diagnostics;
using NetCorePal.Extensions.Primitives.Diagnostics;
using OpenTelemetry.Trace;

namespace NetCorePal.OpenTelemetry.Diagnostics;

public class DiagnosticListener : IObserver<KeyValuePair<string, object?>>
{
    public const string SourceName = "NetCorePal.OpenTelemetry";

    private static readonly ActivitySource ActivitySource = new(SourceName, "1.0.0");

    /// <summary>Notifies the observer that the provider has finished sending push-based notifications.</summary>
    public void OnCompleted()
    {
    }

    /// <summary>Notifies the observer that the provider has experienced an error condition.</summary>
    /// <param name="error">An object that provides additional information about the error.</param>
    public void OnError(Exception error)
    {
    }

    /// <summary>Provides the observer with new data.</summary>
    /// <param name="evt">The current notification information.</param>
    public void OnNext(KeyValuePair<string, object?> evt)
    {
        switch (evt.Key)
        {
            case NetCorePalDiagnosticListenerNames.DomainEventHandlerBegin:
                {
                    var eventData = (DomainEventHandlerBegin)evt.Value!;
                    var activity = ActivitySource.StartActivity("DomainEventHandler: " + eventData.Name);
                    if (activity != null)
                    {
                        activity.SetTag("DomainEvent.Id", eventData.Id);
                        activity.SetTag("DomainEvent.Name", eventData.Name);
                        activity.SetTag("DomainEvent.EventData", System.Text.Json.JsonSerializer.Serialize(eventData.EventData));

                        activity.AddEvent(new ActivityEvent("DomainEventHandlerBegin"));
                    }
                }
                break;
            case NetCorePalDiagnosticListenerNames.DomainEventHandlerEnd:
                {
                    var eventData = (DomainEventHandlerEnd)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetTag("DomainEvent.Id", eventData.Id);
                        activity.SetTag("DomainEvent.Name", eventData.Name);
                        activity.AddEvent(new ActivityEvent("DomainEventHandlerEnd"));
                    }
                    Activity.Current?.Stop();
                }
                break;
            case NetCorePalDiagnosticListenerNames.DomainEventHandlerError:
                {
                    var eventData = (DomainEventHandlerError)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, eventData.Exception.Message);
                        activity.RecordException(eventData.Exception);
                        activity.Stop();
                    }
                }
                break;
            case NetCorePalDiagnosticListenerNames.IntegrationEventHandlerBegin:
                {
                    var eventData = (IntegrationEventHandlerBegin)evt.Value!;
                    var activity = ActivitySource.StartActivity("IntegrationEventHandler: " + eventData.HandlerName);
                    if (activity != null)
                    {
                        activity.SetTag("IntegrationEvent.Id", eventData.Id);
                        activity.SetTag("IntegrationEvent.Name", eventData.HandlerName);
                        activity.SetTag("IntegrationEvent.EventData", System.Text.Json.JsonSerializer.Serialize(eventData.EventData));

                        activity.AddEvent(new ActivityEvent("IntegrationEventHandlerBegin"));
                    }
                }
                break;
            case NetCorePalDiagnosticListenerNames.IntegrationEventHandlerEnd:
                {
                    var eventData = (IntegrationEventHandlerEnd)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetTag("IntegrationEvent.Id", eventData.Id);
                        activity.SetTag("IntegrationEvent.Name", eventData.HandlerName);
                        activity.AddEvent(new ActivityEvent("IntegrationEventHandlerEnd"));
                    }
                    Activity.Current?.Stop();
                }
                break;
            case NetCorePalDiagnosticListenerNames.IntegrationEventHandlerError:
                {
                    var eventData = (IntegrationEventHandlerError)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, eventData.Exception.Message);
                        activity.RecordException(eventData.Exception);
                        activity.Stop();
                    }
                }
                break;
            case NetCorePalDiagnosticListenerNames.TransactionBegin:
                {
                    var eventData = (TransactionBegin)evt.Value!;
                    var activity = ActivitySource.StartActivity("Transaction: " + eventData.TransactionId);
                    if (activity != null)
                    {
                        activity.SetTag("Transaction.Id", eventData.TransactionId);

                        activity.AddEvent(new ActivityEvent("TransactionBegin"));
                    }
                }
                break;
            case NetCorePalDiagnosticListenerNames.TransactionCommit:
                {
                    var eventData = (TransactionCommit)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetTag("Transaction.Id", eventData.TransactionId);
                        activity.AddEvent(new ActivityEvent("TransactionCommit"));
                    }
                    Activity.Current?.Stop();
                }
                break;
            case NetCorePalDiagnosticListenerNames.TransactionRollback:
                {
                    var eventData = (TransactionRollback)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetTag("Transaction.Id", eventData.TransactionId);
                        activity.AddEvent(new ActivityEvent("TransactionRollback"));
                        activity.Stop();
                    }
                }
                break;
            case NetCorePalDiagnosticListenerNames.CommandHandlerBegin:
                {
                    var eventData = (CommandBegin)evt.Value!;
                    var activity = ActivitySource.StartActivity("Command: " + eventData.Name);
                    if (activity != null)
                    {
                        activity.SetTag("CommandBegin.Id", eventData.Id);
                        activity.SetTag("CommandBegin.Name", eventData.Name);
                        activity.SetTag("CommandBegin.EventData", System.Text.Json.JsonSerializer.Serialize(eventData.CommandData));

                        activity.AddEvent(new ActivityEvent("CommandBegin"));
                    }
                }
                break;
            case NetCorePalDiagnosticListenerNames.CommandHandlerEnd:
                {
                    var eventData = (CommandEnd)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetTag("DomainEvent.Id", eventData.Id);
                        activity.SetTag("DomainEvent.Name", eventData.Name);
                        activity.AddEvent(new ActivityEvent("CommandEnd"));
                    }
                    Activity.Current?.Stop();
                }
                break;
            case NetCorePalDiagnosticListenerNames.CommandHandlerError:
                {
                    var eventData = (CommandError)evt.Value!;
                    if (Activity.Current is { } activity)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, eventData.Exception.Message);
                        activity.RecordException(eventData.Exception);
                        activity.Stop();
                    }
                }
                break;
        }
    }
}