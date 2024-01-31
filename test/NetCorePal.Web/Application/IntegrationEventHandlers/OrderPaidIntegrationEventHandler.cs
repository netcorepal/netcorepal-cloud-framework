using MediatR;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Web.Application.IntegrationEventHandlers
{
    public class OrderPaidIntegrationEventHandler(
        IMediator mediator,
        IIntegrationEventPublisher integrationEventPublisher) : IIntegrationEventHandler<OrderPaidIntegrationEvent>
    {
        public async Task HandleAsync(OrderPaidIntegrationEvent eventData,
            CancellationToken cancellationToken = default)
        {
            var cmd = new OrderPaidCommand(eventData.OrderId);
            await mediator.Send(cmd, cancellationToken);
        }
    }
}