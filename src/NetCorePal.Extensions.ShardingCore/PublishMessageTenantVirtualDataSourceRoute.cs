using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public class PublishMessageTenantVirtualDataSourceRoute(ITenantToDatabaseConvertor tenantToDatabaseConvertor)
    : AbstractShardingOperatorVirtualDataSourceRoute<PublishedMessage, string>
{
    private readonly List<string> _dataSources = new List<string>()
    {
        "Db0", "Db1"
    };

    //我们设置区域就是数据库
    public override string ShardingKeyToDataSourceName(object shardingKey)
    {
        return tenantToDatabaseConvertor.Convert(shardingKey?.ToString() ?? string.Empty);
    }

    public override List<string> GetAllDataSourceNames()
    {
        return _dataSources;
    }

    public override bool AddDataSourceName(string dataSourceName)
    {
        if (_dataSources.Any(o => o == dataSourceName))
            return false;
        _dataSources.Add(dataSourceName);
        return true;
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
        builder.ShardingProperty(o => o.TenantId);
    }
}