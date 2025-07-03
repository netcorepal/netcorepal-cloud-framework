using MediatR;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers;

/// <summary>
/// 订单创建领域事件处理器
/// </summary>
public class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderCreatedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单创建事件
        // 例如：发送邮件通知、记录日志等
        
        // 可能触发其他业务流程，比如创建新用户
        // 这里模拟当订单创建后，可能需要创建一个新用户
        await _mediator.Send(new CreateUserCommand("新用户", "user@example.com"), cancellationToken);
    }
}

/// <summary>
/// 订单支付领域事件处理器
/// </summary>
public class OrderPaidDomainEventHandler : IDomainEventHandler<OrderPaidDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderPaidDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderPaidDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单支付事件
        // 例如：更新库存、发送配送指令等
        
        // 支付成功后，可能需要激活用户
        await _mediator.Send(new ActivateUserCommand(new UserId(Guid.NewGuid())), cancellationToken);
    }
}

/// <summary>
/// 订单名称变更领域事件处理器
/// </summary>
public class OrderNameChangedDomainEventHandler : IDomainEventHandler<OrderNameChangedDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderNameChangedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderNameChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单名称变更事件
        
        // 名称变更后，可能需要禁用某个用户
        await _mediator.Send(new DeactivateUserCommand(new UserId(Guid.NewGuid())), cancellationToken);
    }
}

/// <summary>
/// 订单删除领域事件处理器
/// </summary>
public class OrderDeletedDomainEventHandler : IDomainEventHandler<OrderDeletedDomainEvent>
{
    private readonly IMediator _mediator;

    public OrderDeletedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 处理订单删除事件
        
        // 订单删除后，可能需要创建新订单
        await _mediator.Send(new CreateOrderCommand("补偿订单", 0), cancellationToken);
    }
}
