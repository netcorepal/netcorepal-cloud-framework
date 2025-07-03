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
