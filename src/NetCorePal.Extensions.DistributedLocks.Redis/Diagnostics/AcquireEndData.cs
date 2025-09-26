namespace NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

public record AcquireEndData(Guid Id, string Key, string MethodName);