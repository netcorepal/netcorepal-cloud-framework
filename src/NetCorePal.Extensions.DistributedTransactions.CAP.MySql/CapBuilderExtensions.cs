using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCorePal.Extensions.DistributedTransactions.CAP;
using NetCorePal.Extensions.DistributedTransactions.CAP.MySql;

namespace NetCorePal.Extensions.DependencyInjection
{
    public static class CapBuilderExtensions
    {
        /// <summary>
        /// 使用MySql事务处理器
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        [Obsolete("此方法已废弃，请改用 UseNetCorePalStorage 代替。This method is obsolete, please use UseNetCorePalStorage instead.")]
        public static ICapBuilder UseMySql(this ICapBuilder builder)
        {
            if (builder.Services.Any(p => p.ServiceType == typeof(ICapTransactionFactory)))
            {
                throw new InvalidOperationException(R.RepeatAddition);
            }

            builder.Services.TryAddScoped<ICapTransactionFactory, MySqlCapTransactionFactory>();
            return builder;
        }
    }
}