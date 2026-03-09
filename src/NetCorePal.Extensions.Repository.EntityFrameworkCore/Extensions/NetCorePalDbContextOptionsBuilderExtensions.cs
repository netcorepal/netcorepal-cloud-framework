using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

/// <summary>
/// <see cref="DbContextOptionsBuilder"/> 的 NetCorePal 扩展方法。
/// </summary>
public static class NetCorePalDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// 启用 DateTimeOffset 写入数据库前转为 UTC 的补丁（用于兼容 Npgsql/PostgreSQL：timestamp with time zone 仅接受 Offset=0）。
    /// 启用后会对当前 DbContext 下所有未单独配置 ValueConverter 的 DateTimeOffset/DateTimeOffset? 属性生效，与数据库提供程序无关；
    /// 主要使用场景为 Npgsql，其他提供程序下启用也会统一按 UTC 写入，请按需调用。默认不启用。
    /// </summary>
    /// <param name="optionsBuilder">DbContext 选项构建器。</param>
    /// <param name="enable">为 true 时启用补丁（默认 true）。</param>
    /// <returns>同一 <paramref name="optionsBuilder"/>，便于链式调用。</returns>
    public static DbContextOptionsBuilder UseDateTimeOffsetUtcConversionForNpgsql(
        this DbContextOptionsBuilder optionsBuilder,
        bool enable = true)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        var infrastructure = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;
        infrastructure.AddOrUpdateExtension(new NetCorePalDbContextOptionsExtension(enable));

        return optionsBuilder;
    }
}
