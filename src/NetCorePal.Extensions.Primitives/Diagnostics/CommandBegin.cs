namespace NetCorePal.Extensions.Primitives.Diagnostics;

public record CommandBegin(Guid Id, string Name, object CommandData);