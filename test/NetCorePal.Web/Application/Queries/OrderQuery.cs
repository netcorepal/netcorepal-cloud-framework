namespace NetCorePal.Web.Application.Queries
{
    public class OrderQuery(ApplicationDbContext applicationDbContext)
    {

        public async Task<Order?> QueryOrder(OrderId orderId, CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        }
    }
}
