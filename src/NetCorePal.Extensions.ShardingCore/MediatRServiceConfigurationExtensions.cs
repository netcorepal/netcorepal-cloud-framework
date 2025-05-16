using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class MediatRServiceConfigurationExtensions
{
    /// <summary>
    /// 添加Command对于ShardingCore的支持， 必须注册在AddUnitOfWorkBehaviors之前
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static MediatRServiceConfiguration AddShardingBehavior(this MediatRServiceConfiguration cfg)
    {
        cfg.AddOpenBehavior(typeof(ShardingCoreCommandBehavior<,>));
        return cfg;
    }

    /// <summary>
    /// 添加Command对于ShardingCore租户模式的支持， 必须注册在AddUnitOfWorkBehaviors之前
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static MediatRServiceConfiguration AddTenantShardingBehavior(this MediatRServiceConfiguration cfg)
    {
        cfg.AddOpenBehavior(typeof(TenantShardingCommandBehavior<,>));
        return cfg;
    }
}