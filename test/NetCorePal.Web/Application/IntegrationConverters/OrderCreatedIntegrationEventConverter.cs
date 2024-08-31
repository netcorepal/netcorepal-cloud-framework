using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Web.Application.IntegrationEventHandlers;

namespace NetCorePal.Web.Application.IntegrationConverters;

/// <summary>
/// OrderCreatedIntegrationConvert
/// </summary>
public class OrderCreatedIntegrationEventConverter : IIntegrationEventConverter<OrderCreatedDomainEvent,OrderCreatedIntegrationEvent>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="domainEvent"></param>
    /// <returns></returns>
    public OrderCreatedIntegrationEvent Convert(OrderCreatedDomainEvent domainEvent)
    {
        return new OrderCreatedIntegrationEvent(domainEvent.Order.Id);
    }
}