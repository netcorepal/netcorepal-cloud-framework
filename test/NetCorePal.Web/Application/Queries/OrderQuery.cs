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
        public async Task<Order?> QueryOrder(OrderId orderId, CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        }
    }
}
