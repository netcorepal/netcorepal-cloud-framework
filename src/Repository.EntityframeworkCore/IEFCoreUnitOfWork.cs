using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.Repository
{
    public interface IEFCoreUnitOfWork : IUnitOfWork
    {
        IDbContextTransaction BeginTransaction();
    }
}
