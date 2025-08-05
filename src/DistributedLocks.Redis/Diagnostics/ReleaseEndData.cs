namespace NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

public record ReleaseEndData(Guid Id, string MethodName);