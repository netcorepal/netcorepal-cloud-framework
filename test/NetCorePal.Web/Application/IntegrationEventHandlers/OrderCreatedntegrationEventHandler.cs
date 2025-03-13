using MediatR;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Web.Application.IntegrationEventHandlers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public class OrderCreatedIntegrationEventHandler(
        ILogger<OrderCreatedIntegrationEventHandler> logger,IOrderRepository orderRepository) : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        public async Task HandleAsync(OrderCreatedIntegrationEvent eventData,
            CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetAsync(eventData.OrderId, cancellationToken);
            logger.LogInformation("OrderCreatedIntegrationEventHandler.HandleAsync");
        }
    }
}