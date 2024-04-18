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
    public class CreateOrderCommandHandler(IOrderRepository orderRepository, ILogger<CreateOrderCommandHandler> logger)
        : ICommandHandler<CreateOrderCommand, OrderId>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var a = new List<long>();
            for (int i = 0; i < 1000; i++)
            {
                a.Add(i);
            }
            await Parallel.ForEachAsync(a, new ParallelOptions(), async (item,c) =>
            {
                await Task.Delay(12, c);
            });

            var order = new Order(request.Name, request.Count);
            order = await orderRepository.AddAsync(order, cancellationToken);
            logger.LogInformation("order created, id:{orderId}", order.Id);
            return order.Id;
        }
    }
}