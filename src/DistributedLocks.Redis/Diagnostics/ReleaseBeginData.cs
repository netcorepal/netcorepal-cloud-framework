namespace NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

public record ReleaseBeginData(Guid Id, string MethodName);