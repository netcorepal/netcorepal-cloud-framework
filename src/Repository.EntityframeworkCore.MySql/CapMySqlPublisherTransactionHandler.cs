using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.MySql
{
    public class CapMySqlPublisherTransactionHandler : IPublisherTransactionHandler
    {
        private readonly ICapPublisher _capBus;
        public CapMySqlPublisherTransactionHandler(IServiceProvider serviceProvider)
        {
            _capBus = serviceProvider.GetRequiredService<ICapPublisher>();
        }

        public IDbContextTransaction BeginTransaction(EFContext context)
        {
            return context.Database.BeginTransaction(_capBus, autoCommit: false);
        }
    }
}
