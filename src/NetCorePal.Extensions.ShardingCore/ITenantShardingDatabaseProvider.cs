// using ShardingCore.Core.RuntimeContexts;
//
// namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;
//
// public interface ITenantShardingDatabaseProvider
// {
//     List<string> GetAllDatabaseNames();
//     bool AddDataSourceName(string dataSourceName);
// }
// public class SimpleTenantShardingDatabaseProvider(List<string> databaseNames) : ITenantShardingDatabaseProvider
// {
//     public List<string> GetAllDatabaseNames() => databaseNames;
//
//     public bool AddDataSourceName(string dataSourceName)
//     {
//         if (databaseNames.Contains(dataSourceName))
//         {
//             return false;
//         }
//
//         databaseNames.Add(dataSourceName);
//         return true;
//     }
//     
// }