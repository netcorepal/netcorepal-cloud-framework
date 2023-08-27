using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.PostgreSql
{
    public class CapPostgreSqlPublisherTransactionHandler : IPublisherTransactionHandler
    {
        private readonly ICapPublisher _capBus;
        public CapPostgreSqlPublisherTransactionHandler(ICapPublisher capPublisher)
        {
            _capBus = capPublisher;
        }

        public IDbContextTransaction BeginTransaction(EFContext context)
        {
            return context.Database.BeginTransaction(_capBus, autoCommit: false);
        }
    }
}
