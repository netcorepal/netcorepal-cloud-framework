using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;

/// <summary>
/// 订单创建集成事件
/// </summary>
public class OrderCreatedIntegrationEvent : IIntegrationEvent
{
    public Guid OrderId { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 订单支付集成事件
/// </summary>
public class OrderPaidIntegrationEvent : IIntegrationEvent
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
}

/// <summary>
/// 订单删除集成事件
/// </summary>
public class OrderDeletedIntegrationEvent : IIntegrationEvent
{
    public Guid OrderId { get; set; }
    public DateTime DeletedAt { get; set; }
}
