using ShardingCore.Core.ShardingConfigurations.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public static class ShardingRouteConfigOptionsExtensions
{
    public static IShardingRouteConfigOptions AddCapShardingDataSourceRoute(this IShardingRouteConfigOptions options)
    {
        options.AddShardingDataSourceRoute<PublishedMessageVirtualDataSourceRoute>();
        return options;
    }
}