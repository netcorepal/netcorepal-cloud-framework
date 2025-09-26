namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record CommandError(Guid Id, string Name, object CommandData, Exception Exception);