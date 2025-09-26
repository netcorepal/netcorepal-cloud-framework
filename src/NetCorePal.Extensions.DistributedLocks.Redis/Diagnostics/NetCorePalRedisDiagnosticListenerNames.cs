using System.Diagnostics;

namespace NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

public static class NetCorePalRedisDiagnosticListenerNames
{
    public static readonly DiagnosticListener DiagnosticListener =
        new(DiagnosticListenerName);

    public const string DiagnosticListenerName = "NetCorePal.RedisLockDiagnosticListener";

    public const string AcquireBegin = "NetCorePal.RedisLock.AcquireBegin";
    public const string AcquireEnd = "NetCorePal.RedisLock.AcquireEnd";
    public const string AcquireError = "NetCorePal.RedisLock.AcquireErrorData";
    public const string ReleaseBegin = "NetCorePal.RedisLock.ReleaseBegin";
    public const string ReleaseEnd = "NetCorePal.RedisLock.ReleaseEnd";
    public const string ReleaseError = "NetCorePal.RedisLock.ReleaseError";
}