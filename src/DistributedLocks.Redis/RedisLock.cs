using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace NetCorePal.Extensions.DistributedLocks.Redis;

public class RedisLock : IDistributedLock
{
    readonly IDatabase _database;

    public RedisLock(IDatabase database)
    {
        _database = database;
    }

    public ILockSynchronizationHandler? TryAcquire(string key, TimeSpan timeout = default(TimeSpan),
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var rd = new RedisDistributedLock(key, _database);
        var handle = rd.TryAcquire(timeout, cancellationToken);
        return handle == null ? null : new RedisLockSynchronizationHandler(handle);
    }

    public ILockSynchronizationHandler Acquire(string key, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var rd = new RedisDistributedLock(key, _database);
        var handle = rd.Acquire(timeout, cancellationToken);
        return new RedisLockSynchronizationHandler(handle);
    }

    public async ValueTask<ILockSynchronizationHandler?> TryAcquireAsync(string key,
        TimeSpan timeout = default(TimeSpan),
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var rd = new RedisDistributedLock(key, _database);
        var handle = await rd.TryAcquireAsync(timeout, cancellationToken);
        return handle == null ? null : new RedisLockSynchronizationHandler(handle);
    }

    public async ValueTask<ILockSynchronizationHandler> AcquireAsync(string key, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var rd = new RedisDistributedLock(key, _database);
        var handle = await rd.AcquireAsync(timeout, cancellationToken);
        return new RedisLockSynchronizationHandler(handle);
    }
}