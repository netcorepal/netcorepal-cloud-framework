using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer
{
    public class CapSqlServerPublisherTransactionHandler : IPublisherTransactionHandler
    {
        private readonly Lazy<ICapPublisher> _capBus; //lazy load to avoid circular dependency
        public CapSqlServerPublisherTransactionHandler(IServiceProvider serviceProvider)
        {
            _capBus = new Lazy<ICapPublisher>(() => serviceProvider.GetRequiredService<ICapPublisher>());
        }

        public IDbContextTransaction BeginTransaction(DbContext context)
        {
            return context.Database.BeginTransaction(_capBus.Value, autoCommit: false);
        }
    }
}
