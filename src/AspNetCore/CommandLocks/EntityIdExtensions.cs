using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public static class EntityIdExtensions
{
    public static CommandLockSettings ToCommandLockSettings<TId>(this TId id,
        int acquireSeconds = 10)
        where TId : IEntityId
    {
        return new CommandLockSettings(typeof(TId).Name + "-" + id.ToString()!, acquireSeconds: acquireSeconds);
    }

    public static CommandLockSettings ToCommandLockSettings<TId>(this IEnumerable<TId> ids,
        int acquireSeconds = 10)
        where TId : IEntityId
    {
        var typeName = typeof(TId).Name;
        return new CommandLockSettings(ids.Select(id => typeName + "-" + id.ToString()),
            acquireSeconds: acquireSeconds);
    }
}