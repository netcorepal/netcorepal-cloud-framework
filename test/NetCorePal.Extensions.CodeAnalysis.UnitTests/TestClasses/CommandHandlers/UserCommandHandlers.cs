using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.Commands;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.CommandHandlers;

/// <summary>
/// 创建用户命令处理器
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserId>
{
    public Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 创建用户聚合
        var user = new User(new UserId(Guid.NewGuid()), request.Name, request.Email);
        
        // 这里应该保存到仓储，但为了测试我们只是返回ID
        return Task.FromResult(user.Id);
    }
}

/// <summary>
/// 激活用户命令处理器
/// </summary>
public class ActivateUserCommandHandler : ICommandHandler<ActivateUserCommand>
{
    public Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        // 模拟调用聚合方法
        // var user = await userRepository.GetAsync(request.UserId);
        
        // 为了让代码分析器能检测到命令与聚合方法的关系，我们创建一个临时的聚合实例并调用方法
        var user = new User(request.UserId, "Test User", "test@example.com");
        user.Activate();
        
        // await userRepository.SaveAsync(user);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// 禁用用户命令处理器
/// </summary>
public class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
{
    public Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        // 模拟调用聚合方法
        // var user = await userRepository.GetAsync(request.UserId);
        
        // 为了让代码分析器能检测到命令与聚合方法的关系，我们创建一个临时的聚合实例并调用方法
        var user = new User(request.UserId, "Test User", "test@example.com");
        user.Deactivate();
        
        // await userRepository.SaveAsync(user);
        
        return Task.CompletedTask;
    }
}
