using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore
{
    public interface IPublisherTransactionHandler
    {
        IDbContextTransaction BeginTransaction(EFContext context);
    }
}
