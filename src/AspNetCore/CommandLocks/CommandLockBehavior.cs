using MediatR;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public class CommandLockBehavior<TRequest, TResponse>(
    IEnumerable<ICommandLock<TRequest>> commandLocks,
    IDistributedLock distributedLock)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
#pragma warning disable S3604
    private readonly CommandLockedKeysHolder _lockedKeys = new();
#pragma warning restore S3604

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var count = commandLocks.Count();
        if (count == 0)
        {
            return await next(cancellationToken);
        }

        if (count > 1)
        {
            throw new InvalidOperationException(R.OnlyOneCommandLockAllowed);
        }

        var commandLock = commandLocks.First();
        var options = await commandLock.GetLockKeysAsync(request, cancellationToken);
        if (!string.IsNullOrEmpty(options.LockKey))
        {
            if (!_lockedKeys.LockedKeys.Keys.Contains(options.LockKey))
            {
                await using var lockHandler =
                    await TryAcquireAsync(options.LockKey, timeout: options.AcquireTimeout,
                        cancellationToken: cancellationToken);
                if (lockHandler == null)
                {
                    throw new CommandLockFailedException(NetCorePal.Extensions.Primitives.R.AcquireLockFailed, options.LockKey);
                }

                _lockedKeys.LockedKeys.Keys.Add(options.LockKey);
                // 确保在执行next后，释放锁
                return await next(cancellationToken);
            }
            else
            {
                return await next(cancellationToken);
            }
        }
        else
        {
            return await LockAndRelease(options, 0, next, cancellationToken);
        }
    }


    private async Task<TResponse> LockAndRelease(CommandLockSettings settings, int lockIndex,
        RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (lockIndex >= settings.LockKeys!.Count)
        {
            return await next(cancellationToken);
        }

        var key = settings.LockKeys[lockIndex];

        if (!_lockedKeys.LockedKeys.Keys.Contains(key))
        {
            await using var lockHandler =
                await TryAcquireAsync(key, timeout: settings.AcquireTimeout,
                    cancellationToken: cancellationToken);
            if (lockHandler == null)
            {
                throw new CommandLockFailedException(NetCorePal.Extensions.Primitives.R.AcquireLockFailed, key);
            }

            _lockedKeys.LockedKeys.Keys.Add(key);
            // 确保在执行LockAndRelease后，释放锁
            return await LockAndRelease(settings, lockIndex + 1, next, cancellationToken);
        }
        else
        {
            return await LockAndRelease(settings, lockIndex + 1, next, cancellationToken);
        }
    }

    private async Task<LockSynchronizationHandlerWarpper?> TryAcquireAsync(string key, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var handler = await distributedLock.TryAcquireAsync(key, timeout: timeout,
            cancellationToken: cancellationToken);
        if (handler == null)
        {
            return null;
        }

        return new LockSynchronizationHandlerWarpper(key, _lockedKeys.LockedKeys.Keys, handler);
    }
}