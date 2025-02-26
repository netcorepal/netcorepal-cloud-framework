using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Dto;
using NetCorePal.Web.Controllers;

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
                .Select(p => new OrderQueryResult(p.Id, p.Name, p.Count, p.Paid, p.CreateTime, p.RowVersion, p.UpdateAt,
                    p.Deleted, p.DeletedTime,
                    p.OrderItems.Select(x => new OrderItemQueryResult(x.Id, x.Name, x.Count, x.RowVersion))))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// 查询列表，支持分页
        /// </summary>
        /// <param name="name">合同名关键字</param>
        /// <param name="index">页码</param>
        /// <param name="size">每页数量</param>
        /// <param name="countTotal">是否需要总数</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedData<OrderQueryResult>> ListOrderByPage(string? name, int index, int size,
            bool countTotal, CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders
                .Where(x => string.IsNullOrEmpty(name) || x.Name.Contains(name))
                .Select(p => new OrderQueryResult(p.Id, p.Name, p.Count, p.Paid, p.CreateTime, p.RowVersion, p.UpdateAt,
                    p.Deleted, p.DeletedTime,
                    p.OrderItems.Select(x => new OrderItemQueryResult(x.Id, x.Name, x.Count, x.RowVersion))))
                .ToPagedDataAsync(index, size, countTotal, cancellationToken);
        }

        /// <summary>
        /// 查询列表，支持分页。同步版本
        /// </summary>
        /// <param name="name">合同名关键字</param>
        /// <param name="index">页码</param>
        /// <param name="size">每页数量</param>
        /// <param name="countTotal">是否需要总数</param>
        /// <returns></returns>
        public PagedData<OrderQueryResult> ListOrderByPageSync(string? name, int index, int size, bool countTotal)
        {
            return applicationDbContext.Orders
                .Where(x => string.IsNullOrEmpty(name) || x.Name.Contains(name))
                .Select(p => new OrderQueryResult(p.Id, p.Name, p.Count, p.Paid, p.CreateTime, p.RowVersion, p.UpdateAt,
                    p.Deleted, p.DeletedTime,
                    p.OrderItems.Select(x => new OrderItemQueryResult(x.Id, x.Name, x.Count, x.RowVersion))))
                .ToPagedData(index, size, countTotal);
        }

        /// <summary>
        /// 查询列表，支持分页
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedData<OrderQueryResult>> ListOrderByPageRequestAsync(ListOrdersRequest request,
            CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders
                .Where(x => string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name))
                .Select(p => new OrderQueryResult(p.Id, p.Name, p.Count, p.Paid, p.CreateTime, p.RowVersion, p.UpdateAt,
                    p.Deleted, p.DeletedTime,
                    p.OrderItems.Select(x => new OrderItemQueryResult(x.Id, x.Name, x.Count, x.RowVersion))))
                .ToPagedDataAsync(request, cancellationToken);
        }

        /// <summary>
        /// 查询列表，支持分页。同步版本
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PagedData<OrderQueryResult> ListOrderByPageRequest(ListOrdersRequest request)
        {
            return applicationDbContext.Orders
                .Where(x => string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name))
                .Select(p => new OrderQueryResult(p.Id, p.Name, p.Count, p.Paid, p.CreateTime, p.RowVersion, p.UpdateAt,
                    p.Deleted, p.DeletedTime,
                    p.OrderItems.Select(x => new OrderItemQueryResult(x.Id, x.Name, x.Count, x.RowVersion))))
                .ToPagedData(request);
        }

        public async Task<OrderQueryResult> GetOrderIgnoreQueryFilter(OrderId id, CancellationToken cancellationToken)
        {
            return await applicationDbContext.Orders
                .IgnoreQueryFilters()
                .Where(x => x.Id == id)
                .Select(p => new OrderQueryResult(p.Id, p.Name, p.Count, p.Paid, p.CreateTime, p.RowVersion, p.UpdateAt,
                    p.Deleted, p.DeletedTime,
                    p.OrderItems.Select(x => new OrderItemQueryResult(x.Id, x.Name, x.Count, x.RowVersion))))
                .SingleAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="OrderId"></param>
    /// <param name="Name"></param>
    /// <param name="Count"></param>
    /// <param name="Paid"></param>
    /// <param name="CreateTime"></param>
    /// <param name="RowVersion"></param>
    /// <param name="UpdateAt"></param>
    /// <param name="OrderItems"></param>
    public record OrderQueryResult(
        OrderId OrderId,
        string Name,
        int Count,
        bool Paid,
        DateTimeOffset CreateTime,
        RowVersion RowVersion,
        UpdateTime UpdateAt,
        Deleted Deleted,
        DeletedTime DeletedTime,
        IEnumerable<OrderItemQueryResult> OrderItems);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="OrderItemId"></param>
    /// <param name="Name"></param>
    /// <param name="Count"></param>
    /// <param name="RowVersion"></param>
    public record OrderItemQueryResult(
        OrderItemId OrderItemId,
        string Name,
        int Count,
        RowVersion RowVersion);
}