using MediatR;
using NetCorePal.Extensions.Domain;

namespace ABC.Template.Web.Application.DomainEventHandlers
{
    internal class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
    {
        readonly IMediator _mediator;

        public OrderCreatedDomainEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            return _mediator.Send(new DeliverGoodsCommand(notification.Order.Id), cancellationToken);
        }
    }
}
