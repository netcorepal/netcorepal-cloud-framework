namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record IntegrationEventHandlerError(Guid Id, string HandlerName, object EventData, Exception Exception);