using System.Reflection.Metadata;

namespace NetCorePal.Extensions.Primitives;

public sealed record CommandLockSettings
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lockKey"></param>
    /// <param name="acquireSeconds"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandLockSettings(string lockKey,
        int acquireSeconds = 10)
    {
        if (string.IsNullOrEmpty(lockKey))
        {
            throw new ArgumentNullException(nameof(lockKey));
        }

        this.LockKey = lockKey;
        this.AcquireTimeout = TimeSpan.FromSeconds(acquireSeconds);
    }

    public CommandLockSettings(IEnumerable<string> lockKeys,
        int acquireSeconds = 10)
    {
        if (lockKeys == null)
        {
            throw new ArgumentNullException(nameof(lockKeys));
        }

        var sortedSet = new SortedSet<string>();

        foreach (var key in lockKeys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("lockKeys contains empty string", nameof(lockKeys));
            }

            if (!sortedSet.Add(key))
            {
                throw new ArgumentException("lockKeys contains duplicate key", nameof(lockKeys));
            }
        }

        if (sortedSet.Count == 0)
        {
            throw new ArgumentException("lockKeys is empty", nameof(lockKeys));
        }

        LockKeys = sortedSet.ToList();
        this.AcquireTimeout = TimeSpan.FromSeconds(acquireSeconds);
    }


    /// <summary>
    /// 要加锁的key，如果要加锁多个key，请使用LockKeys
    /// </summary>
    public string? LockKey { get; private set; }

    /// <summary>
    /// 要加锁的列表，当LockKey为空时使用
    /// </summary>
    public IReadOnlyList<string>? LockKeys { get; private set; }

    /// <summary>key
    /// 获取锁的等待时间（秒）
    /// </summary>
    public TimeSpan AcquireTimeout { get; private set; }
}