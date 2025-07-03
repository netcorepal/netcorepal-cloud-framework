using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers;

/// <summary>
/// 订单创建集成事件处理器
/// </summary>
public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public Task HandleAsync(OrderCreatedIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理订单创建集成事件
        // 例如：同步到其他系统、发送通知等
        return Task.CompletedTask;
    }
}

/// <summary>
/// 订单支付集成事件处理器
/// </summary>
public class OrderPaidIntegrationEventHandler : IIntegrationEventHandler<OrderPaidIntegrationEvent>
{
    public Task HandleAsync(OrderPaidIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理订单支付集成事件
        // 例如：通知库存系统、财务系统等
        return Task.CompletedTask;
    }
}

/// <summary>
/// 订单删除集成事件处理器
/// </summary>
public class OrderDeletedIntegrationEventHandler : IIntegrationEventHandler<OrderDeletedIntegrationEvent>
{
    public Task HandleAsync(OrderDeletedIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理订单删除集成事件
        return Task.CompletedTask;
    }
}
