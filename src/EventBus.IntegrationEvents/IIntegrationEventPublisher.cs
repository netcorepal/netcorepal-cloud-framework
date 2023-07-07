using NetCorePal.Extensions.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.EventBus.IntegrationEvents
{
    public class IntegrationEventPublisher<TIntegrationEvent> : IDomainEventHandler<TIntegrationEvent> where TIntegrationEvent : class, IDomainEvent, IIntegrationEvent
    {
        readonly IEventPublisher _eventPublisher;
        public IntegrationEventPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }


        public async Task Handle(TIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await _eventPublisher.PublishAsync(nameof(TIntegrationEvent), notification, cancellationToken);
        }
    }
}
