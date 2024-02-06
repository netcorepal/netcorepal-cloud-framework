using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Infra.Repositories;

namespace NetCorePal.Web.Application.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="orderRepository"></param>
    public class OrderPaidCommandHandler(IOrderRepository orderRepository) : ICommandHandler<OrderPaidCommand>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="KnownException"></exception>
        public async Task Handle(OrderPaidCommand request, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetAsync(request.OrderId, cancellationToken) ?? throw new KnownException($"未找到订单，OrderId = {request.OrderId}");
            order.OrderPaid();
        }
    }
}
