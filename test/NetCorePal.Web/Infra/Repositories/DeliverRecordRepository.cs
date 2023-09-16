using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;

namespace NetCorePal.Web.Infra.Repositories
{
    public interface IDeliverRecordRepository : IRepository<DeliverRecord, DeliverRecordId>
    {

    }

    public class DeliverRecordRepository : RepositoryBase<DeliverRecord, DeliverRecordId, ApplicationDbContext>, IDeliverRecordRepository
    {
        public DeliverRecordRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
