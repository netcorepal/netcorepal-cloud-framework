using NetCorePal.Extensions.Mappers;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Application.IntegrationEventHandlers;

namespace NetCorePal.Web.Application.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="orderRepository"></param>
    /// <param name="logger"></param>
    public class CreateOrderCommandHandler(IOrderRepository orderRepository, ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, OrderId>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order(request.Name, request.Count);
            order = await orderRepository.AddAsync(order, cancellationToken);
            logger.LogInformation("order created, id:{orderId}", order.Id);
            return order.Id;
        }
    }
}
