using System.Reflection.Metadata;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public sealed class CommandLockSettings
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lockKey"></param>
    /// <param name="acquireSeconds"></param>
    /// <param name="errorMessage"></param>
    /// <param name="errorCode"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandLockSettings(string lockKey,
        int acquireSeconds = 10,
        string? errorMessage = null,
        int errorCode = 500)
    {
        if (string.IsNullOrEmpty(lockKey))
        {
            throw new ArgumentNullException(nameof(lockKey));
        }

        this.LockKey = lockKey;
        Init(acquireSeconds, errorMessage, errorCode);
    }

    public CommandLockSettings(IEnumerable<string> lockKeys,
        int acquireSeconds = 10,
        string? errorMessage = null,
        int errorCode = 500)
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

        Init(acquireSeconds, errorMessage, errorCode);
    }


    private void Init(int acquireSeconds, string? errorMessage, int errorCode)
    {
        this.AcquireTimeout = TimeSpan.FromSeconds(acquireSeconds);
        if (errorMessage != null)
        {
            this.ErrorMessage = errorMessage;
        }

        this.ErrorCode = errorCode;
    }


    /// <summary>
    /// 要加锁的key，如果要加锁多个key，请使用LockKeys
    /// </summary>
    public string? LockKey { get; private set; }

    /// <summary>
    /// 要加锁的列表，当LockKey为空时使用
    /// </summary>
    public IReadOnlyList<string>? LockKeys { get; private set; }

    public TimeSpan AcquireTimeout { get; private set; }
    public string ErrorMessage { get; private set; } = "获取锁超时";
    public int ErrorCode { get; private set; } = 500;
}