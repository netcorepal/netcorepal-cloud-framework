using MediatR;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Web.Application.IntegrationEventHandlers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="integrationEventPublisher"></param>
    public class OrderPaidIntegrationEventHandler(
        IMediator mediator,
        IIntegrationEventPublisher integrationEventPublisher) : IIntegrationEventHandler<OrderPaidIntegrationEvent>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="cancellationToken"></param>
        public async Task HandleAsync(OrderPaidIntegrationEvent eventData,
            CancellationToken cancellationToken = default)
        {
            var cmd = new OrderPaidCommand(eventData.OrderId);
            await mediator.Send(cmd, cancellationToken);
        }
    }
}