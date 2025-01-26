using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{
    public interface IPublisherTransactionHandler
    {
        ValueTask<IDbContextTransaction> BeginTransactionAsync(DbContext context);
    }
}