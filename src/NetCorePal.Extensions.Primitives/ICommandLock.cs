namespace NetCorePal.Extensions.Primitives;

/// <summary>
/// 表示一个命令锁
/// </summary>
/// <typeparam name="TCommand">要锁定的命令类型</typeparam>
public interface ICommandLock<in TCommand> where TCommand : IBaseCommand
{
    Task<CommandLockSettings> GetLockKeysAsync(TCommand command,
        CancellationToken cancellationToken = default);
}