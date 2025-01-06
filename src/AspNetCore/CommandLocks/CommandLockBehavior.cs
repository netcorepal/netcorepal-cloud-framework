using MediatR;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public class CommandLockBehavior<TRequest, TResponse>(
    ICommandLock<TRequest> commandLock,
    IDistributedLock distributedLock)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    private readonly CommandLockedKeysHolder _lockedKeys = new();


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
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
                    throw new CommandLockFailedException("Acquire Lock Faild", options.LockKey);
                }

                _lockedKeys.LockedKeys.Keys.Add(options.LockKey);
                // 确保在执行next后，释放锁
                return await next();
            }
            else
            {
                return await next();
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
            return await next();
        }

        var key = settings.LockKeys[lockIndex];

        if (!_lockedKeys.LockedKeys.Keys.Contains(key))
        {
            await using var lockHandler =
                await TryAcquireAsync(key, timeout: settings.AcquireTimeout,
                    cancellationToken: cancellationToken);
            if (lockHandler == null)
            {
                throw new CommandLockFailedException("Acquire Lock Faild", key);
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