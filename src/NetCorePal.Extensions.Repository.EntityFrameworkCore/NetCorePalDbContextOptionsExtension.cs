using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

/// <summary>
/// EF Core DbContext 扩展，用于存储 NetCorePal 相关选项（如 DateTimeOffset 写入 PostgreSQL 时转为 UTC 的开关）。
/// </summary>
public class NetCorePalDbContextOptionsExtension : IDbContextOptionsExtension
{
    private readonly DbContextOptionsExtensionInfo _info;

    /// <summary>
    /// 是否在写入数据库前将 DateTimeOffset 转为 UTC（用于兼容 Npgsql 仅接受 Offset=0）。
    /// </summary>
    public bool EnableDateTimeOffsetUtcConversion { get; }

    /// <summary>
    /// 创建扩展实例。
    /// </summary>
    /// <param name="enableDateTimeOffsetUtcConversion">为 true 时，所有 DateTimeOffset/DateTimeOffset? 属性在写入前转为 UTC。</param>
    public NetCorePalDbContextOptionsExtension(bool enableDateTimeOffsetUtcConversion = true)
    {
        EnableDateTimeOffsetUtcConversion = enableDateTimeOffsetUtcConversion;
        _info = new NetCorePalDbContextOptionsExtensionInfo(this);
    }

    /// <inheritdoc />
    public DbContextOptionsExtensionInfo Info => _info;

    /// <inheritdoc />
    public void ApplyServices(IServiceCollection services)
    {
        // 无需注册服务，仅在 OnModelCreating 时读取选项
    }

    /// <inheritdoc />
    public void Validate(IDbContextOptions options)
    {
        // 无额外校验
    }

    /// <inheritdoc />
    public string LogFragment => EnableDateTimeOffsetUtcConversion
        ? "DateTimeOffsetUtcConversionForNpgsql "
        : string.Empty;
}
