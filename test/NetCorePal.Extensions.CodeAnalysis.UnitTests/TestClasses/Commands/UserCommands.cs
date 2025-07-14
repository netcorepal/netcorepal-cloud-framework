using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

/// <summary>
/// 创建用户命令
/// </summary>
public record CreateUserCommand(string Name, string Email) : ICommand<UserId>;

/// <summary>
/// 激活用户命令
/// </summary>
public record ActivateUserCommand(UserId UserId) : ICommand;

/// <summary>
/// 禁用用户命令
/// </summary>
public record DeactivateUserCommand(UserId UserId) : ICommand;

/// <summary>
/// 更新用户资料命令
/// </summary>
public record UpdateUserProfileCommand(UserId UserId, string Name, string Email) : ICommand
{
    // 无参构造函数用于测试
    public UpdateUserProfileCommand() : this(new UserId(Guid.NewGuid()), "Updated Name", "updated@email.com")
    {
    }
}
