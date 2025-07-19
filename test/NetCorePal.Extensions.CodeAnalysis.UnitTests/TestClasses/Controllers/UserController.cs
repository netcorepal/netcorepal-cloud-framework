using MediatR;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
public class UserController
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    public async Task<UserId> CreateUser(CreateUserCommand command)
    {
        var userId = await _mediator.Send(command);
        return userId;
    }

    /// <summary>
    /// 激活用户
    /// </summary>
    public async Task ActivateUser(Guid userId)
    {
        await _mediator.Send(new ActivateUserCommand(new UserId(userId)));
    }

    /// <summary>
    /// 禁用用户
    /// </summary>
    public async Task DeactivateUser(Guid userId)
    {
        await _mediator.Send(new DeactivateUserCommand(new UserId(userId)));
    }

    /// <summary>
    /// 获取用户
    /// </summary>
    public Task<User> GetUser(Guid userId)
    {
        // 模拟获取用户
        return Task.FromResult<User>(null!);
    }
}
