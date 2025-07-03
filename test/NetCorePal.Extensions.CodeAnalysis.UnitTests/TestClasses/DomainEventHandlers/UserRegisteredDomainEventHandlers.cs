using MediatR;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEventHandlers;

/// <summary>
/// 用户注册欢迎邮件处理器
/// </summary>
public class UserRegisteredWelcomeEmailHandler : IDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly IMediator _mediator;

    public UserRegisteredWelcomeEmailHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        // 发送欢迎邮件命令
        await _mediator.Send(new CreateUserCommand("欢迎用户", "welcome@example.com"), cancellationToken);
    }
}

/// <summary>
/// 用户注册统计处理器
/// </summary>
public class UserRegisteredStatisticsHandler : IDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly IMediator _mediator;

    public UserRegisteredStatisticsHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        // 更新统计数据
        await _mediator.Send(new ActivateUserCommand(notification.User.Id), cancellationToken);
    }
}

/// <summary>
/// 用户注册默认设置处理器
/// </summary>
public class UserRegisteredDefaultSettingsHandler : IDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly IMediator _mediator;

    public UserRegisteredDefaultSettingsHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        // 设置默认用户设置
        await _mediator.Send(new DeactivateUserCommand(notification.User.Id), cancellationToken);
    }
}
