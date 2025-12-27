using AsyncKeyedLock;

namespace NetCorePal.Extensions.DistributedLocks;

/// <summary>
/// A simple in-memory implementation of IDistributedLock for tests and single-process scenarios.
/// NOT suitable for multi-process or multi-machine use.
/// </summary>
public sealed class InMemoryDistributedLock : IDistributedLock
{
    private static readonly AsyncKeyedLocker<string> Locks = new();

    public ILockSynchronizationHandler? TryAcquire(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        // default(TimeSpan) means immediate try (0)
        var waitTime = timeout == default ? TimeSpan.Zero : timeout;
        var acquired = Locks.LockOrNull(key, waitTime, cancellationToken);
        return acquired is null ? null : new Handle(acquired);
    }

    public ILockSynchronizationHandler Acquire(string key, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (timeout is null)
        {
            var semLock = Locks.Lock(key, cancellationToken);
            return new Handle(semLock);
        }

        var timeoutLock = Locks.LockOrNull(key, timeout.Value, cancellationToken)
            ?? throw new TimeoutException($"Failed to acquire lock for key '{key}' within {timeout.Value}.");

        return new Handle(timeoutLock);
    }

    public async ValueTask<ILockSynchronizationHandler?> TryAcquireAsync(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        // default(TimeSpan) means immediate try (0)
        var waitTime = timeout == default ? TimeSpan.Zero : timeout;
        var acquired = await Locks.LockOrNullAsync(key, waitTime, cancellationToken).ConfigureAwait(false);
        return acquired is null ? null : new Handle(acquired);
    }

    public async ValueTask<ILockSynchronizationHandler> AcquireAsync(string key, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (timeout is null)
        {
            var semLock = await Locks.LockAsync(key, cancellationToken).ConfigureAwait(false);
            return new Handle(semLock);
        }

        var timeoutLock = await Locks.LockOrNullAsync(key, timeout.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new TimeoutException($"Failed to acquire lock for key '{key}' within {timeout.Value}.");
        
        return new Handle(timeoutLock);
    }

    private sealed class Handle(IDisposable disposable) : ILockSynchronizationHandler
    {
        private readonly IDisposable? _disposable = disposable;
        private readonly CancellationTokenSource _cts = new();

        public CancellationToken HandleLostToken => _cts.Token; // In-memory lock doesn't lose ownership

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
