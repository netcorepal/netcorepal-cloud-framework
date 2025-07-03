using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers;

/// <summary>
/// 订单创建领域事件处理器
/// </summary>
public class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单创建事件
        // 例如：发送邮件通知、记录日志等
        return Task.CompletedTask;
    }
}

/// <summary>
/// 订单支付领域事件处理器
/// </summary>
public class OrderPaidDomainEventHandler : IDomainEventHandler<OrderPaidDomainEvent>
{
    public Task Handle(OrderPaidDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单支付事件
        // 例如：更新库存、发送配送指令等
        return Task.CompletedTask;
    }
}

/// <summary>
/// 订单名称变更领域事件处理器
/// </summary>
public class OrderNameChangedDomainEventHandler : IDomainEventHandler<OrderNameChangedDomainEvent>
{
    public Task Handle(OrderNameChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单名称变更事件
        return Task.CompletedTask;
    }
}

/// <summary>
/// 订单删除领域事件处理器
/// </summary>
public class OrderDeletedDomainEventHandler : IDomainEventHandler<OrderDeletedDomainEvent>
{
    public Task Handle(OrderDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单删除事件
        return Task.CompletedTask;
    }
}
