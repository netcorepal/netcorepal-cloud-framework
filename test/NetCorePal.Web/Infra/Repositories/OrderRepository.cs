using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Infra.Repositories
{

    /// <summary>
    /// 
    /// </summary>
    public interface IOrderRepository : IRepository<Order, OrderId>
    {

    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public class OrderRepository(ApplicationDbContext context) : RepositoryBase<Order, OrderId, ApplicationDbContext>(context), IOrderRepository
    {
    }
}
