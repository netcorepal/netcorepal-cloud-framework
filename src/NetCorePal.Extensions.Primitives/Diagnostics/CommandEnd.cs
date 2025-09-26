namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record CommandEnd(Guid Id, string Name, object CommandData);