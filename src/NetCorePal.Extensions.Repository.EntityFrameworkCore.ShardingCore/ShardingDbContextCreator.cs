// using Microsoft.EntityFrameworkCore;
// using ShardingCore.Core.DbContextCreator;
// using ShardingCore.Core.RuntimeContexts;
// using ShardingCore.Core.ServiceProviders;
// using ShardingCore.Sharding.Abstractions;
//
// namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;
//
// public class ShardingDbContextCreator<MyDbContext> : IDbContextCreator where MyDbContext :  IShardingTableDbContext
// {
//     public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
//     {
//         var outDbContext = (MyDbContext)shellDbContext;
//         var dbContext = new MyDbContext(shardingDbContextOptions.DbContextOptions,outDbContext.OtherParams);
//         if (dbContext is IShardingTableDbContext shardingTableDbContext)
//         {
//             shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
//         }
//         _ = dbContext.Model;
//         return dbContext;
//     }
//
//     public DbContext GetShellDbContext(IShardingProvider shardingProvider)
//     {
//         IShardingRuntimeContext shardingRuntimeContext = shardingProvider.GetRequiredService<IShardingRuntimeContext>();
//         var dbContext = shardingRuntimeContext.GetDbContext<MyDbContext>();
//     }
// }