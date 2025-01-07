using NetCorePal.Extensions.DistributedLocks;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

/// <summary>
/// 在释放锁后，从持有者集合中移除key
/// </summary>
/// <param name="key"></param>
/// <param name="holder"></param>
/// <param name="handler"></param>
class LockSynchronizationHandlerWarpper(string key, HashSet<string> holder, ILockSynchronizationHandler handler)
    : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await handler.DisposeAsync();
        holder.Remove(key);
    }
}