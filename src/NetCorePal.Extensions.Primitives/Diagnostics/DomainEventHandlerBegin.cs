namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record DomainEventHandlerBegin(Guid Id, string Name, object EventData);