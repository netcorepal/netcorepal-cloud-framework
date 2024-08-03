# 悲观锁-Redis锁

## 什么是Redis锁？

`Redis锁`是一种用于解决并发问题的机制，基于Redis的`悲观锁`实现。在并发环境中，多个用户可能同时变更同一资源，如果不加以限制，不同用户之间的操作可能相互覆盖，导致数据不一致的问题。`Redis锁`就是为了解决这个问题而设计的。

## Redis锁的实现原理

`Redis锁`的实现原理是通过在Redis中设置一个唯一的键来表示锁，当一个线程获取锁时，其他线程无法获取锁，直到该线程释放锁。具体步骤如下：
1. 线程A尝试获取锁，在Redis中设置一个唯一键，如果设置成功功，则获取锁。
2. 线程A执行完操作后，删除该唯一键，释放锁。
3. 线程B尝试获取锁，如果Redis中不存在该唯一键，则获取锁，否则等待或重试。

## Redis锁的使用场景

`Redis锁`主要用于解决高并发问题，例如在订单系统中，当多个用户同时下单时，可能会导致超卖的问题，此时可以使用`Redis锁`使得扣除库存的操作每次只能有一个线程执行，确保数据的一致性。

## 注册Redis锁

在Program.cs注册`IDistributedLock`：
```csharp
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.DistributedLocks.Redis;
using StackExchange.Redis;

var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
builder.Services.AddSingleton<IConnectionMultiplexer>(p => redis);
builder.Services.AddRedisLocks();
```

## 使用Redis锁

在需要使用锁的地方注入`IDistributedLock`：
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