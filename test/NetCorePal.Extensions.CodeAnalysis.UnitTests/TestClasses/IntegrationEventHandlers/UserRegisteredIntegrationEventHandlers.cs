using MediatR;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers;

/// <summary>
/// 用户注册CRM同步处理器
/// </summary>
public class UserRegisteredCrmSyncHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserRegisteredCrmSyncHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 同步到CRM系统
        await _mediator.Send(new CreateOrderCommand(new UserId(Guid.NewGuid()), 100, "CRM订单"), cancellationToken);
    }
}

/// <summary>
/// 用户注册营销系统处理器
/// </summary>
public class UserRegisteredMarketingHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserRegisteredMarketingHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 添加到营销活动
        await _mediator.Send(new ChangeOrderNameCommand(new OrderId(Guid.NewGuid()), "营销订单"), cancellationToken);
    }
}

/// <summary>
/// 用户注册推送通知处理器
/// </summary>
public class UserRegisteredPushNotificationHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserRegisteredPushNotificationHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 发送推送通知
        await _mediator.Send(new DeleteOrderCommand(new OrderId(Guid.NewGuid())), cancellationToken);
    }
}
