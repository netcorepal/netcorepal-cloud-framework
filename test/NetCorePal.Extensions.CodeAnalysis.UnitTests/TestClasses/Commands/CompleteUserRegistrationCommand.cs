using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

/// <summary>
/// 完成用户注册命令
/// </summary>
public record CompleteUserRegistrationCommand(UserId UserId) : ICommand;
