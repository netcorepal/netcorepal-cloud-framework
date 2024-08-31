using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.Domain;
using NetCorePal.Web.Application.IntegrationConverters;

namespace ABC.Template.Web.Application.DomainEventHandlers;

public class OrderCreatedIntegrationEventConverterTestDomainEventHandler(IIntegrationEventPublisher integrationEventPublisher,
    OrderCreatedIntegrationEventConverter convert) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken){
        // 发出转移操作集成事件
        var integrationEvent = convert.Convert(notification);
        await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken);
    }
}