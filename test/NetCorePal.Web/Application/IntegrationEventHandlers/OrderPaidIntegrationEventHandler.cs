using MediatR;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Web.Application.IntegrationEventHandlers
{
    public class OrderPaidIntegrationEventHandler : IIntegrationEventHandler<OrderPaidIntegrationEvent>
    {
        readonly ILogger _logger;
        readonly IMediator _mediator;
        public OrderPaidIntegrationEventHandler(IMediator mediator, ILogger<OrderPaidIntegrationEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task HandleAsync(OrderPaidIntegrationEvent eventData, CancellationToken cancellationToken = default)
        {
            var cmd = new OrderPaidCommand(eventData.OrderId);
            return _mediator.Send(cmd);
        }

    }
}
