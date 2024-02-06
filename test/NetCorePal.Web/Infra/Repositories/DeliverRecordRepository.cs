using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Web.Infra.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public class DeliverRecordRepository(ApplicationDbContext context) : RepositoryBase<DeliverRecord, DeliverRecordId, ApplicationDbContext>(context)
    {
    }
}
