using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class CapBuilderExtensions
    {
        /// <summary>
        /// 使用PostgreSql事务处理器
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        [Obsolete("此方法已废弃，不再需要显式调用数据库特定的事务处理器。框架会自动使用通用的事务处理器。This method is obsolete and no longer needed. The framework will automatically use the generic transaction handler.")]
        public static ICapBuilder UsePostgreSql(this ICapBuilder builder)
        {
            if (builder.Services.Any(p => p.ServiceType == typeof(ICapTransactionFactory)))
            {
                throw new InvalidOperationException(R.RepeatAddition);
            }

            builder.Services.TryAddScoped<ICapTransactionFactory, PostgreSqlCapTransactionFactory>();
            return builder;
        }
    }
}