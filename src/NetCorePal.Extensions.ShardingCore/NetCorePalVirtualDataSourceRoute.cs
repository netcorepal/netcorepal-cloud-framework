using Microsoft.Extensions.Options;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

/// <summary>
/// 租户数据源路由
/// </summary>
/// <param name="options"></param>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class NetCorePalVirtualDataSourceRoute<TEntity, TKey>(
    IOptions<NetCorePalShardingCoreOptions> options) : AbstractShardingOperatorVirtualDataSourceRoute<TEntity, TKey>
    where TEntity : class
{
    private static readonly PublishedMessageDataSourceContext ShardingDatabaseContext =
        new PublishedMessageDataSourceContext();

    private NetCorePalShardingCoreOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="shardingKey"></param>
    /// <returns></returns>
    protected abstract string GetDataSourceName(object? shardingKey);


    public override string ShardingKeyToDataSourceName(object? shardingKey)
    {
        var dataSourceName = GetDataSourceName(shardingKey);
        if (!string.IsNullOrEmpty(dataSourceName))
        {
            ShardingDatabaseContext.SetDataSourceName(dataSourceName);
        }

        return dataSourceName;
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