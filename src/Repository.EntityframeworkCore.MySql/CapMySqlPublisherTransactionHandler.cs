using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.MySql
{
    public class CapMySqlPublisherTransactionHandler : IPublisherTransactionHandler
    {
        private readonly ICapPublisher _capBus;
        public CapMySqlPublisherTransactionHandler(ICapPublisher capPublisher)
        {
            _capBus = capPublisher;
        }

        public IDbContextTransaction BeginTransaction(EFContext context)
        {
            return context.Database.BeginTransaction(_capBus, autoCommit: false);
        }
    }
}
