using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

/// <summary>
/// <see cref="NetCorePalDbContextOptionsExtension"/> 的元数据信息。
/// </summary>
public class NetCorePalDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
{
    private readonly NetCorePalDbContextOptionsExtension _extension;

    /// <summary>
    /// 创建扩展信息实例。
    /// </summary>
    public NetCorePalDbContextOptionsExtensionInfo(IDbContextOptionsExtension extension)
        : base(extension)
    {
        _extension = (NetCorePalDbContextOptionsExtension)extension;
    }

    /// <inheritdoc />
    public override bool IsDatabaseProvider => false;

    /// <inheritdoc />
    public override string LogFragment => _extension.EnableDateTimeOffsetUtcConversion
        ? "DateTimeOffsetUtcConversionForNpgsql "
        : string.Empty;

    /// <inheritdoc />
    public override int GetServiceProviderHashCode() => 0;

    /// <inheritdoc />
    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;

    /// <inheritdoc />
    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
        debugInfo["NetCorePal:EnableDateTimeOffsetUtcConversion"] = _extension.EnableDateTimeOffsetUtcConversion ? "1" : "0";
    }
}
