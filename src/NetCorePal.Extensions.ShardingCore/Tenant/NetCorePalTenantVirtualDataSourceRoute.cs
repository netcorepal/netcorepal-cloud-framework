using Microsoft.Extensions.Options;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Tenant;

/// <summary>
/// 租户数据源路由
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class NetCorePalTenantVirtualDataSourceRoute<TEntity, TKey>(
    IOptions<NetCorePalShardingCoreOptions> options,
    ITenantDataSourceProvider provider)
    : AbstractShardingOperatorVirtualDataSourceRoute<TEntity, TKey>
    where TEntity : class
{
    private static readonly PublishedMessageDataSourceContext ShardingDatabaseContext =
        new PublishedMessageDataSourceContext();

    private readonly NetCorePalShardingCoreOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly ITenantDataSourceProvider _tenantDataSourceProvider = provider ?? throw new ArgumentNullException(nameof(provider));

    public override string ShardingKeyToDataSourceName(object? shardingKey)
    {
        return _tenantDataSourceProvider.GetDataSourceName(shardingKey?.ToString() ?? string.Empty);
    }

    public override List<string> GetAllDataSourceNames()
    {
        return _options.AllDataSourceNames;
    }

    public override bool AddDataSourceName(string dataSourceName)
    {
        throw new NotImplementedException();
    }

    public override Func<string, bool> GetRouteToFilter(TKey shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var t = ShardingKeyToDataSourceName(shardingKey!);
        switch (shardingOperator)
        {
            case ShardingOperatorEnum.Equal: return tail => tail == t;
            default:
            {
                return tail => true;
            }
        }
    }
}