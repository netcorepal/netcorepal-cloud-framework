using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventConverters;

/// <summary>
/// 订单创建领域事件到集成事件转换器
/// </summary>
public class OrderCreatedIntegrationEventConverter : IIntegrationEventConverter<OrderCreatedDomainEvent, OrderCreatedIntegrationEvent>
{
    public OrderCreatedIntegrationEvent Convert(OrderCreatedDomainEvent domainEvent)
    {
        return new OrderCreatedIntegrationEvent
        {
            OrderId = domainEvent.Order.Id.Id,
            OrderName = domainEvent.Order.Name,
            Price = domainEvent.Order.Price,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 订单支付领域事件到集成事件转换器
/// </summary>
public class OrderPaidIntegrationEventConverter : IIntegrationEventConverter<OrderPaidDomainEvent, OrderPaidIntegrationEvent>
{
    public OrderPaidIntegrationEvent Convert(OrderPaidDomainEvent domainEvent)
    {
        return new OrderPaidIntegrationEvent
        {
            OrderId = domainEvent.Order.Id.Id,
            Amount = domainEvent.Order.Price,
            PaidAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 订单支付领域事件到订单支付集成事件转换器
/// </summary>
public class OrderPaidToOrderPaidIntegrationEventConverter : IIntegrationEventConverter<OrderPaidDomainEvent, OrderPaidIntegrationEvent>
{
    public OrderPaidIntegrationEvent Convert(OrderPaidDomainEvent domainEvent)
    {
        return new OrderPaidIntegrationEvent
        {
            OrderId = domainEvent.Order.Id.Id,
            Amount = domainEvent.Order.Price,
            PaidAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 订单支付领域事件到订单删除集成事件转换器
/// </summary>
public class OrderPaidToOrderDeletedIntegrationEventConverter : IIntegrationEventConverter<OrderPaidDomainEvent, OrderDeletedIntegrationEvent>
{
    public OrderDeletedIntegrationEvent Convert(OrderPaidDomainEvent domainEvent)
    {
        return new OrderDeletedIntegrationEvent
        {
            OrderId = domainEvent.Order.Id.Id,
            DeletedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 订单创建领域事件到订单创建集成事件转换器
/// </summary>
public class OrderCreatedToOrderCreatedIntegrationEventConverter : IIntegrationEventConverter<OrderCreatedDomainEvent, OrderCreatedIntegrationEvent>
{
    public OrderCreatedIntegrationEvent Convert(OrderCreatedDomainEvent domainEvent)
    {
        return new OrderCreatedIntegrationEvent
        {
            OrderId = domainEvent.Order.Id.Id,
            OrderName = domainEvent.Order.Name,
            Price = domainEvent.Order.Price,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 用户创建领域事件到订单创建集成事件转换器
/// </summary>
public class UserCreatedToOrderCreatedIntegrationEventConverter : IIntegrationEventConverter<UserCreatedDomainEvent, OrderCreatedIntegrationEvent>
{
    public OrderCreatedIntegrationEvent Convert(UserCreatedDomainEvent domainEvent)
    {
        return new OrderCreatedIntegrationEvent
        {
            OrderId = Guid.NewGuid(),
            OrderName = "用户创建时产生的订单",
            Price = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
}
