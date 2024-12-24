using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public interface ICommandLock<TCommand, TCommandResult> where TCommand : IBaseCommand
{
    Task<CommandLockOptions<TCommandResult>> GetCommandLockOptionsAsync(TCommand command,
        CancellationToken cancellationToken = default);
}

public class CommandLockOptions<TCommandResult>
{
    public string LockKey { get; set; } = "";
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan WaitTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public string ErrorMessage { get; set; } = "获取锁超时";

    public Func<CancellationToken, Task<TCommandResult>>? ErrorMessageFactory { get; set; }
}