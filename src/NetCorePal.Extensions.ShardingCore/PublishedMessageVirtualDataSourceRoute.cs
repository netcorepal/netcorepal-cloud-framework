using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

class PublishedMessageVirtualDataSourceRoute(IOptions<NetCorePalShardingCoreOptions> options)
    : AbstractShardingOperatorVirtualDataSourceRoute<PublishedMessage, string>
{
    public override string ShardingKeyToDataSourceName(object shardingKey)
    {
        return shardingKey.ToString() ?? string.Empty;
    }

    public override List<string> GetAllDataSourceNames()
    {
        return options.Value.AllDataSourceNames;
    }

    public override bool AddDataSourceName(string dataSourceName)
    {
        throw new System.NotImplementedException();
    }

    public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
    {
        var t = ShardingKeyToDataSourceName(shardingKey);
        switch (shardingOperator)
        {
            case ShardingOperatorEnum.Equal: return tail => tail == t;
            default:
            {
                return tail => true;
            }
        }
    }

    public override void Configure(EntityMetadataDataSourceBuilder<PublishedMessage> builder)
    {
        builder.ShardingProperty(o => o.DataSourceName);
    }
}