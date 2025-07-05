using MediatR;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers;

/// <summary>
/// 订单创建集成事件处理器
/// </summary>
public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleAsync(OrderCreatedIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理订单创建集成事件
        // 例如：同步到其他系统、发送通知等
        
        // 集成事件处理时，可能需要创建用户
        await _mediator.Send(new CreateUserCommand("集成用户", "integration@example.com"), cancellationToken);
    }
}

/// <summary>
/// 订单支付集成事件处理器
/// </summary>
public class OrderPaidIntegrationEventHandler : IIntegrationEventHandler<OrderPaidIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderPaidIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleAsync(OrderPaidIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理订单支付集成事件
        // 例如：通知库存系统、财务系统等
        
        // 支付完成后，可能需要更改其他订单的名称
        await _mediator.Send(new ChangeOrderNameCommand(new OrderId(Guid.NewGuid()), "已支付关联订单"), cancellationToken);
    }
}

/// <summary>
/// 订单删除集成事件处理器
/// </summary>
public class OrderDeletedIntegrationEventHandler : IIntegrationEventHandler<OrderDeletedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderDeletedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleAsync(OrderDeletedIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理订单删除集成事件
        
        // 订单删除后，可能需要删除其他相关订单
        await _mediator.Send(new DeleteOrderCommand(new OrderId(Guid.NewGuid())), cancellationToken);
    }
}
