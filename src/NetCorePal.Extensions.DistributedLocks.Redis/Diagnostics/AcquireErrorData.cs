namespace NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

public record AcquireErrorData(Guid Id, string Key, string MethodName, Exception Exception);