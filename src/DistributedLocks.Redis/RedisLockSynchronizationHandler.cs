using Medallion.Threading;
using NetCorePal.Extensions.DistributedLocks;

namespace NetCorePal.Extensions.DistributedLocks.Redis;

internal class RedisLockSynchronizationHandler : ILockSynchronizationHandler
{
    private readonly IDistributedSynchronizationHandle _handle;

    public RedisLockSynchronizationHandler(IDistributedSynchronizationHandle handle)
    {
        _handle = handle;
    }

    public CancellationToken HandleLostToken => _handle.HandleLostToken;

    public void Dispose()
    {
        _handle.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _handle.DisposeAsync();
    }
}