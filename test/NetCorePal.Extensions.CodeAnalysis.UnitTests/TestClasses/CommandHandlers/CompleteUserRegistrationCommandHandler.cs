using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.CommandHandlers;

/// <summary>
/// 完成用户注册命令处理器
/// </summary>
public class CompleteUserRegistrationCommandHandler : ICommandHandler<CompleteUserRegistrationCommand>
{
    public Task Handle(CompleteUserRegistrationCommand request, CancellationToken cancellationToken)
    {
        // 模拟调用聚合方法
        var user = new User(request.UserId, "Test User", "test@example.com");
        user.CompleteRegistration();
        
        return Task.CompletedTask;
    }
}
