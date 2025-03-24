using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class CapBuilderExtensions
    {
        /// <summary>
        /// 使用SqlServer事务处理器
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ICapBuilder UseSqlServer(this ICapBuilder builder)
        {
            builder.Services.TryAddScoped<ICapTransactionFactory, SqlServerCapTransactionFactory>();
            return builder;
        }
    }
}