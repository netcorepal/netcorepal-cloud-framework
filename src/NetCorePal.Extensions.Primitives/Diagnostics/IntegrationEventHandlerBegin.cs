namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record IntegrationEventHandlerBegin(Guid Id, string HandlerName, object EventData);