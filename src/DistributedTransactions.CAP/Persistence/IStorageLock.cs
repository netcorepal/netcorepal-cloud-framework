namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public interface IStorageLock
{
    Task<bool> AcquireLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = default(CancellationToken));

    Task ReleaseLockAsync(string key, string instance, CancellationToken token = default(CancellationToken));

    Task RenewLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = default(CancellationToken));
}

class EmptyStorageLock : IStorageLock
{
    public Task<bool> AcquireLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = default(CancellationToken))
    {
        return Task.FromResult(true);
    }

    public Task ReleaseLockAsync(string key, string instance, CancellationToken token = default(CancellationToken))
    {
        return Task.CompletedTask;
    }

    public Task RenewLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = default(CancellationToken))
    {
        return Task.CompletedTask;
    }
}