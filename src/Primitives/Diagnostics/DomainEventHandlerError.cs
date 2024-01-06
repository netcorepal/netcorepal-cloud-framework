namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record DomainEventHandlerError(Guid Id, string Name, object EventData, Exception Exception);