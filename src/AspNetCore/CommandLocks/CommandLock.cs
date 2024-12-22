using MediatR;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public sealed class CommandLockBehavior<TRequest, TResponse>(
    ICommandLock<TRequest, TResponse> commandLock,
    IDistributedLock
        distributedLock)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var options = await commandLock.GetCommandLockOptionsAsync(request, cancellationToken);
        await using var lockHandler =
            await distributedLock.TryAcquireAsync(options.LockKey, timeout: options.LockTimeout,
                cancellationToken: cancellationToken);
        if (lockHandler == null)
        {
            if (options.ErrorMessageFactory != null)
            {
                return await options.ErrorMessageFactory(cancellationToken);
            }
            else
            {
                throw new KnownException(message: options.ErrorMessage, errorCode: 400);
            }
        }

        return await next();
    }
}