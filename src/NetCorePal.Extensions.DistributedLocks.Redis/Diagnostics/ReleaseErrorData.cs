namespace NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

public record ReleaseErrorData(Guid Id, string MethodName, Exception Exception);