# Pessimistic Lock - Redis Lock

## What is Redis Lock?

`Redis Lock` is a mechanism used to solve concurrency issues, implemented based on Redis's `pessimistic lock`. In a concurrent environment, multiple users may change the same resource simultaneously. If not restricted, operations between different users may overwrite each other, leading to data inconsistency. `Redis Lock` is designed to solve this problem.

## Implementation Principle of Redis Lock

The implementation principle of `Redis Lock` is to set a unique key in Redis to represent the lock. When a thread acquires the lock, other threads cannot acquire the lock until the thread releases the lock. The specific steps are as follows:
1. Thread A tries to acquire the lock by setting a unique key in Redis. If successful, it acquires the lock.
2. After thread A completes the operation, it deletes the unique key to release the lock.
3. Thread B tries to acquire the lock. If the unique key does not exist in Redis, it acquires the lock; otherwise, it waits or retries.

## Usage Scenarios of Redis Lock

`Redis Lock` is mainly used to solve high concurrency issues. For example, in an order system, when multiple users place orders simultaneously, it may lead to overselling. At this time, `Redis Lock` can be used to ensure that the operation of deducting inventory is executed by only one thread at a time, ensuring data consistency.

## Register Redis Lock

Register `IDistributedLock` in `Program.cs`:
```csharp
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.DistributedLocks.Redis;
using StackExchange.Redis;

var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
builder.Services.AddSingleton<IConnectionMultiplexer>(p => redis);
builder.Services.AddRedisLocks();
```

## Use Redis Lock

Inject `IDistributedLock` where the lock is needed:
```csharp
using NetCorePal.Extensions.DistributedLocks;
namespace DistributedLocksSample
{
    public class RedisLockSample
    {
        private readonly IDistributedLock _lock;
        public RedisLockSample(IDistributedLock @lock)
        {
            _lock = @lock;
        }

        public async Task LockSample()
        {
            using (var l = await _lock.Acquire("lock-key", TimeSpan.FromSeconds(10)))
            {
                // do something
            }
        }
        
        public async Task TryLockSample()
        {
            var lockerHandler = await _lock.TryAcquire("lock-key", TimeSpan.FromSeconds(10)))
            if (lockerHandler != null)
            {
                using (lockerHandler)
                {
                    // do something
                }
            }
        }
    }
}
```
