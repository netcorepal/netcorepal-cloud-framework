using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.MySql
{
    public class CapMySqlPublisherTransactionHandler : IPublisherTransactionHandler
    {
        private readonly Lazy<ICapPublisher> _capBus; //lazy load to avoid circular dependency
        public CapMySqlPublisherTransactionHandler(IServiceProvider serviceProvider)
        {
            _capBus = new Lazy<ICapPublisher>(() => serviceProvider.GetRequiredService<ICapPublisher>());
        }

        public IDbContextTransaction BeginTransaction(DbContext context)
        {
            return context.Database.BeginTransaction(_capBus.Value, autoCommit: false);
        }
    }
}
