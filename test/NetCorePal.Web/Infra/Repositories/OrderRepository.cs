using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityframeworkCore;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Infra.Repositories
{

    public interface IOrderRepository : IRepository<Order, OrderId>
    {

    }


    public class OrderRepository : RepositoryBase<Order, OrderId, ApplicationDbContext>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
