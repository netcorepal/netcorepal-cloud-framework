namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record IntegrationEventHandlerEnd(Guid Id, string HandlerName, object EventData);