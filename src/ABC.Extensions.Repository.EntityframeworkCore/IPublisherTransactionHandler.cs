using Microsoft.EntityFrameworkCore.Storage;

namespace ABC.Extensions.Repository.EntityframeworkCore
{
    public interface IPublisherTransactionHandler
    {
        IDbContextTransaction BeginTransaction(EFContext context);
    }
}
