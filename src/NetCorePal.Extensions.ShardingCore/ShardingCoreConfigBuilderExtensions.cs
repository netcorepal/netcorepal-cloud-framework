using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingConfigurations.ConfigBuilders;
using ShardingCore.Sharding.Abstractions;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public static class ShardingCoreConfigBuilderExtensions
{
    public static ShardingCoreConfigBuilder<TShardingDbContext> UseNetCorePal<TShardingDbContext>(
        this ShardingCoreConfigBuilder<TShardingDbContext> builder,Action<NetCorePalShardingCoreOptions> optionsAction)
        where TShardingDbContext : DbContext, IShardingDbContext, IShardingCore
    {
        builder.AddServiceConfigure(services =>
        {
            services.Configure<NetCorePalShardingCoreOptions>(optionsAction);
        });
        return builder;
    }
}