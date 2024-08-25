using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Web.Application.IntegrationEventHandlers;

namespace NetCorePal.Web.Application.IntegrationConvert;

/// <summary>
/// OrderCreatedIntegrationConvert
/// </summary>
public class OrderCreatedIntegrationConvert : IIntegrationEventConvert<OrderCreatedDomainEvent,OrderPaidIntegrationEvent>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="domainEvent"></param>
    /// <returns></returns>
    public OrderPaidIntegrationEvent Convert(OrderCreatedDomainEvent domainEvent)
    {
        return new OrderPaidIntegrationEvent(domainEvent.Order.Id);
    }
}