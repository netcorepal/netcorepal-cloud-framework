namespace NetCorePal.Extensions.Primitives.Diagnostics;

public static class NetCorePalDiagnosticListenerNames
{
    public const string DiagnosticListenerName = "NetCorePal";

    public const string DomainEventHandlerBegin = "NetCorePal.DomainEventHandlerBegin";
    public const string DomainEventHandlerEnd = "NetCorePal.DomainEventHandlerEnd";
    public const string DomainEventHandlerError = "NetCorePal.DomainEventHandlerError";
    
    public const string IntegrationEventHandlerBegin = "NetCorePal.IntegrationEventHandlerBegin";
    public const string IntegrationEventHandlerEnd = "NetCorePal.IntegrationEventHandlerEnd";
    public const string IntegrationEventHandlerError = "NetCorePal.IntegrationEventHandlerError";

    public const string TransactionBegin = "NetCorePal.TransactionBegin";
    public const string TransactionCommit = "NetCorePal.TransactionCommit";
    public const string TransactionRollback = "NetCorePal.TransactionRollback";

    public const string CommandHandlerBegin = "NetCorePal.CommandHandlerBegin";
    public const string CommandHandlerEnd = "NetCorePal.CommandHandlerEnd";
    public const string CommandHandlerError = "NetCorePal.CommandHandlerError";
}