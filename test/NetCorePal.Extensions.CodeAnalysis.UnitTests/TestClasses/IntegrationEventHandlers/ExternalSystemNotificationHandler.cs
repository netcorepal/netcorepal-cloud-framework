using MediatR;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.IntegrationEventHandlers;

/// <summary>
/// 外部系统通知处理器
/// 这个处理器处理的集成事件没有对应的转换器，应该被识别为链路起点
/// </summary>
public class ExternalSystemNotificationHandler : IIntegrationEventHandler<ExternalSystemNotificationEvent>
{
    private readonly IMediator _mediator;

    public ExternalSystemNotificationHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleAsync(ExternalSystemNotificationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 根据通知类型执行不同的业务逻辑
        if (eventData.EventType == "ORDER_UPDATE")
        {
            await _mediator.Send(new ChangeOrderNameCommand(new OrderId(Guid.NewGuid()), "Updated Order"), cancellationToken);
        }
        else if (eventData.EventType == "USER_PROFILE_CHANGE")
        {
            await _mediator.Send(new ActivateUserCommand(new UserId(Guid.NewGuid())), cancellationToken);
        }
    }
}
