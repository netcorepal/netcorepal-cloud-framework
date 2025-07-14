using MediatR;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers;

/// <summary>
/// 订单项添加领域事件处理器
/// </summary>
public class OrderItemAddedDomainEventHandler : IDomainEventHandler<OrderItemAddedDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderItemAddedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderItemAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单项添加事件
        // 例如：更新库存、发送通知等
        
        // 模拟：订单项添加后，可能需要创建新用户（业务逻辑示例）
        await _mediator.Send(new CreateUserCommand("新用户", "user@example.com"), cancellationToken);
    }
}

/// <summary>
/// 订单项数量变更领域事件处理器
/// </summary>
public class OrderItemQuantityChangedDomainEventHandler : IDomainEventHandler<OrderItemQuantityChangedDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderItemQuantityChangedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderItemQuantityChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单项数量变更事件
        // 例如：更新库存、重新计算价格等
        
        // 模拟：数量变更后，可能需要激活用户（业务逻辑示例）
        await _mediator.Send(new ActivateUserCommand(new UserId(Guid.NewGuid())), cancellationToken);
    }
}

/// <summary>
/// 订单项价格变更领域事件处理器
/// </summary>
public class OrderItemPriceChangedDomainEventHandler : IDomainEventHandler<OrderItemPriceChangedDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderItemPriceChangedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderItemPriceChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单项价格变更事件
        // 例如：更新总价、发送价格变更通知等
        
        // 模拟：价格变更后，可能需要禁用用户（业务逻辑示例）
        await _mediator.Send(new DeactivateUserCommand(new UserId(Guid.NewGuid())), cancellationToken);
    }
}

/// <summary>
/// 订单项移除领域事件处理器
/// </summary>
public class OrderItemRemovedDomainEventHandler : IDomainEventHandler<OrderItemRemovedDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderItemRemovedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderItemRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单项移除事件
        // 例如：释放库存、发送通知等
        
        // 模拟：订单项移除后，可能需要删除订单（业务逻辑示例）
        await _mediator.Send(new DeleteOrderCommand(new OrderId(Guid.NewGuid())), cancellationToken);
    }
}
