using NetCorePal.Extensions.Mappers;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Application.IntegrationEventHandlers;

namespace NetCorePal.Web.Application.Commands
{
    public class CreateOrderCommandHandler(IOrderRepository orderRepository, IMapperProvider mapperProvider, ILogger<OrderPaidIntegrationEventHandler> logger) : ICommandHandler<CreateOrderCommand, OrderId>
    {
        public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = new Order(request.Name, request.Price);
            order = await orderRepository.AddAsync(order, cancellationToken);
            logger.LogInformation("order created, id:{orderId}", order.Id);
            return order.Id;
        }
    }
}
