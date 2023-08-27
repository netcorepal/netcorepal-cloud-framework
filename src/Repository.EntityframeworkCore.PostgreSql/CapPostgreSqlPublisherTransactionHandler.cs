using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.PostgreSql
{
    public class CapPostgreSqlPublisherTransactionHandler : IPublisherTransactionHandler
    {
        private readonly ICapPublisher _capBus;
        public CapPostgreSqlPublisherTransactionHandler(IServiceProvider serviceProvider)
        {
            _capBus = serviceProvider.GetRequiredService<ICapPublisher>();
        }

        public IDbContextTransaction BeginTransaction(EFContext context)
        {
            return context.Database.BeginTransaction(_capBus, autoCommit: false);
        }
    }
}
