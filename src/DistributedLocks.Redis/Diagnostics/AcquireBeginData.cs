namespace NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

public record AcquireBeginData(Guid Id, string Key, string MethodName);