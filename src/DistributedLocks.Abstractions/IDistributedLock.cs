namespace NetCorePal.Extensions.DistributedLocks;

public interface IDistributedLock
{
    ILockSynchronizationHandler? TryAcquire(
        string key,
        TimeSpan timeout = default(TimeSpan),
        CancellationToken cancellationToken = default(CancellationToken));

    ILockSynchronizationHandler Acquire(
        string key,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken));

    ValueTask<ILockSynchronizationHandler?> TryAcquireAsync(string key,
        TimeSpan timeout = default(TimeSpan),
        CancellationToken cancellationToken = default(CancellationToken));

    ValueTask<ILockSynchronizationHandler> AcquireAsync(string key,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken));
}