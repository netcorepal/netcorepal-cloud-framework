using MediatR;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public sealed class CommandLockBehavior<TRequest, TResponse>(
    ICommandLock<TRequest> commandLock,
    IDistributedLock distributedLock)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var options = await commandLock.GetCommandLockOptionsAsync(request, cancellationToken);

        if (!string.IsNullOrEmpty(options.LockKey))
        {
            await using var lockHandler =
                await distributedLock.TryAcquireAsync(options.LockKey, timeout: options.AcquireTimeout,
                    cancellationToken: cancellationToken);
            if (lockHandler == null)
            {
                throw new CommandLockFailedException("Acquire Lock Faild");
            }

            return await next();
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

        await using var lockHandler =
            await distributedLock.TryAcquireAsync(settings.LockKeys[lockIndex], timeout: settings.AcquireTimeout,
                cancellationToken: cancellationToken);
        if (lockHandler == null)
        {
            throw new CommandLockFailedException("Acquire Lock Faild");
        }

        return await LockAndRelease(settings, lockIndex + 1, next, cancellationToken);
    }
}