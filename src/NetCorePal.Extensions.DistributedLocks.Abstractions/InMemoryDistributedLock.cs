using System.Collections.Concurrent;

namespace NetCorePal.Extensions.DistributedLocks;

/// <summary>
/// A simple in-memory implementation of IDistributedLock for tests and single-process scenarios.
/// NOT suitable for multi-process or multi-machine use.
/// </summary>
public sealed class InMemoryDistributedLock : IDistributedLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public ILockSynchronizationHandler? TryAcquire(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var sem = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        // default(TimeSpan) means immediate try (0)
        var waitTime = timeout == default ? TimeSpan.Zero : timeout;
        var acquired = sem.Wait(waitTime, cancellationToken);
        return acquired ? new Handle(sem) : null;
    }

    public ILockSynchronizationHandler Acquire(string key, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var sem = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        if (timeout is null)
        {
            sem.Wait(cancellationToken);
        }
        else if (!sem.Wait(timeout.Value, cancellationToken))
        {
            throw new TimeoutException($"Failed to acquire lock for key '{key}' within {timeout.Value}.");
        }

        return new Handle(sem);
    }

    public async ValueTask<ILockSynchronizationHandler?> TryAcquireAsync(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var sem = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        var waitTime = timeout == default ? TimeSpan.Zero : timeout;
        if (await sem.WaitAsync(waitTime, cancellationToken).ConfigureAwait(false))
        {
            return new Handle(sem);
        }
        return null;
    }

    public async ValueTask<ILockSynchronizationHandler> AcquireAsync(string key, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var sem = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        if (timeout is null)
        {
            await sem.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (!await sem.WaitAsync(timeout.Value, cancellationToken).ConfigureAwait(false))
        {
            throw new TimeoutException($"Failed to acquire lock for key '{key}' within {timeout.Value}.");
        }

        return new Handle(sem);
    }

    private sealed class Handle : ILockSynchronizationHandler
    {
        private SemaphoreSlim? _sem;
        private readonly CancellationTokenSource _cts = new();

        public Handle(SemaphoreSlim sem)
        {
            _sem = sem;
        }

        public CancellationToken HandleLostToken => _cts.Token; // In-memory lock doesn't lose ownership

        public void Dispose()
        {
            var sem = Interlocked.Exchange(ref _sem, null);
            if (sem != null)
            {
                try { sem.Release(); } catch { /* ignore over-release */ }
            }
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
