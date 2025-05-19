using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using ShardingCore.Core.ShardingConfigurations.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public static class ShardingRouteConfigOptionsExtensions
{
    /// <summary>
    /// 使CAP的PublishedMessage支持分库
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IShardingRouteConfigOptions AddCapShardingDataSourceRoute(this IShardingRouteConfigOptions options)
    {
        options.AddShardingDataSourceRoute<PublishedMessageVirtualDataSourceRoute>();
        NetCorePalStorageOptions.PublishedMessageShardingDatabaseEnabled = true;
        return options;
    }
}