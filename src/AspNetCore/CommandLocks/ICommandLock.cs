using MediatR;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public interface ICommandLock<in TCommand> where TCommand : IBaseCommand
{
    Task<CommandLockSettings> GetLockKeysAsync(TCommand command,
        CancellationToken cancellationToken = default);
}