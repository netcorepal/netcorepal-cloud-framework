namespace NetCorePal.Web.Application.Queries
{
    public class OrderQuery
    {
        readonly ApplicationDbContext _applicationDbContext;
        public OrderQuery(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Order?> QueryOrder(OrderId orderId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        }
    }
}
