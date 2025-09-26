using System.Diagnostics;
using Medallion.Threading.Redis;
using NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;
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
        var id = Guid.NewGuid();
        try
        {
            var acquireBeginData = new AcquireBeginData(id, key, "TryAcquire");
            WriteAcquireBeginData(acquireBeginData);
            var handle = rd.TryAcquire(timeout, cancellationToken);
            WriteAcquireEndData(new AcquireEndData(id, key, "TryAcquire"));
            return handle == null ? null : new RedisLockSynchronizationHandler(handle);
        }
        catch (Exception e)
        {
            WriteAcquireErrorData(new AcquireErrorData(id, key, "TryAcquire", e));
            throw;
        }
    }

    public ILockSynchronizationHandler Acquire(string key, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var rd = new RedisDistributedLock(key, _database);
        var id = Guid.NewGuid();
        try
        {
            var acquireBeginData = new AcquireBeginData(id, key, "Acquire");
            WriteAcquireBeginData(acquireBeginData);
            var handle = rd.Acquire(timeout, cancellationToken);
            WriteAcquireEndData(new AcquireEndData(id, key, "Acquire"));
            return new RedisLockSynchronizationHandler(handle);
        }
        catch (Exception e)
        {
            WriteAcquireErrorData(new AcquireErrorData(id, key, "Acquire", e));
            throw;
        }
    }

    public async ValueTask<ILockSynchronizationHandler?> TryAcquireAsync(string key,
        TimeSpan timeout = default(TimeSpan),
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var rd = new RedisDistributedLock(key, _database);
        var id = Guid.NewGuid();
        try
        {
            var acquireBeginData = new AcquireBeginData(id, key, "TryAcquireAsync");
            WriteAcquireBeginData(acquireBeginData);
            var handle = await rd.TryAcquireAsync(timeout, cancellationToken);
            WriteAcquireEndData(new AcquireEndData(id, key, "TryAcquireAsync"));
            return handle == null ? null : new RedisLockSynchronizationHandler(handle);
        }
        catch (Exception e)
        {
            WriteAcquireErrorData(new AcquireErrorData(id, key, "TryAcquireAsync", e));
            throw;
        }
    }

    public async ValueTask<ILockSynchronizationHandler> AcquireAsync(string key, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var rd = new RedisDistributedLock(key, _database);
        var id = Guid.NewGuid();
        try
        {
            var acquireBeginData = new AcquireBeginData(id, key, "AcquireAsync");
            WriteAcquireBeginData(acquireBeginData);
            var handle = await rd.AcquireAsync(timeout, cancellationToken);
            WriteAcquireEndData(new AcquireEndData(id, key, "AcquireAsync"));
            return new RedisLockSynchronizationHandler(handle);
        }
        catch (Exception e)
        {
            WriteAcquireErrorData(new AcquireErrorData(id, key, "AcquireAsync", e));
            throw;
        }
    }


    void WriteAcquireBeginData(AcquireBeginData data)
    {
        if (NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalRedisDiagnosticListenerNames
                .AcquireBegin))
        {
            NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NetCorePalRedisDiagnosticListenerNames.AcquireBegin, data);
        }
    }

    void WriteAcquireEndData(AcquireEndData data)
    {
        if (NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalRedisDiagnosticListenerNames
                .AcquireEnd))
        {
            NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NetCorePalRedisDiagnosticListenerNames.AcquireEnd, data);
        }
    }


    void WriteAcquireErrorData(AcquireErrorData data)
    {
        if (NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalRedisDiagnosticListenerNames
                .AcquireEnd))
        {
            NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NetCorePalRedisDiagnosticListenerNames.AcquireError, data);
        }
    }
}