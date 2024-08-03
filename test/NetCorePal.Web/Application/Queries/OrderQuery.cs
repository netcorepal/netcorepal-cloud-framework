using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Application.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationDbContext"></param>
    public class OrderQuery(ApplicationDbContext applicationDbContext)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OrderQueryResult?> QueryOrder(OrderId orderId, CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders.Where(x => x.Id == orderId)
                .Select(p => new OrderQueryResult(p.Id, p.Name, p.Count, p.Paid, p.CreateTime, p.RowVersion,
                    p.OrderItems.Select(x => new OrderItemQueryResult(x.Id, x.Name, x.Count, x.RowVersion))))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }

    public record OrderQueryResult(
        OrderId OrderId,
        string Name,
        int Count,
        bool Paid,
        DateTime CreateTime,
        RowVersion RowVersion,
        IEnumerable<OrderItemQueryResult> OrderItems);


    public record OrderItemQueryResult(
        OrderItemId OrderItemId,
        string Name,
        int Count,
        RowVersion RowVersion);
}