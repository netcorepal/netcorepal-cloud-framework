using MediatR;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 测试没有发出任何命令的CommandSender
/// </summary>
public class TestEmptyCommandSender
{
    private readonly IMediator _mediator;

    public TestEmptyCommandSender(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 只执行查询操作，不发出命令
    /// </summary>
    public Task<string> QueryOnly()
    {
        // 只查询，不发出命令
        return Task.FromResult("Query Result");
    }

    /// <summary>
    /// 没有任何操作的方法
    /// </summary>
    public void DoNothing()
    {
        // 空方法，不发出命令
    }
}

/// <summary>
/// 测试复杂命令发送场景的CommandSender
/// </summary>
public class TestAdvancedCommandSender(IMediator mediator)
{
    /// <summary>
    /// 根据条件发送不同的命令
    /// </summary>
    public async Task<string> ConditionalSend(string type, CancellationToken cancellationToken = default)
    {
        return type.ToLower() switch
        {
            "record" => await mediator.Send(new RecordCommandWithResult("Record Type"), cancellationToken),
            "class" => await SendClassCommand(cancellationToken),
            _ => "Unknown Type"
        };
    }

    /// <summary>
    /// 私有方法发送命令
    /// </summary>
    private async Task<string> SendClassCommand(CancellationToken cancellationToken = default)
    {
        await mediator.Send(new ClassCommandWithOutResult { Name = "Private Method" }, cancellationToken);
        return "Class Command Sent";
    }

    /// <summary>
    /// 异步并行发送多个命令
    /// </summary>
    public async Task SendParallelCommands(CancellationToken cancellationToken = default)
    {
        var task1 = mediator.Send(new RecordCommandWithResult("Parallel 1"), cancellationToken);
        var task2 = mediator.Send(new RecordCommandWithResult("Parallel 2"), cancellationToken);
        var task3 = mediator.Send(new ClassCommandWithOutResult { Name = "Parallel 3" }, cancellationToken);

        await Task.WhenAll(task1, task2, task3);
    }

    /// <summary>
    /// 使用泛型方法发送命令
    /// </summary>
    public async Task<T> SendGenericCommand<T>(T data, CancellationToken cancellationToken = default) where T : class
    {
        await mediator.Send(new RecordCommandWithResult($"Generic: {typeof(T).Name}"), cancellationToken);
        return data;
    }

    /// <summary>
    /// 在异常处理中发送命令
    /// </summary>
    public async Task<string> SendWithErrorHandling(CancellationToken cancellationToken = default)
    {
        try
        {
            return await mediator.Send(new RecordCommandWithResult("Try Block"), cancellationToken);
        }
        catch (Exception ex)
        {
            await mediator.Send(new ClassCommandWithOutResult { Name = $"Error: {ex.GetType().Name}" }, cancellationToken);
            throw;
        }
        finally
        {
            await mediator.Send(new RecordCommandWithResult("Finally Block"), cancellationToken);
        }
    }
}

/// <summary>
/// 测试继承场景的CommandSender
/// </summary>
public abstract class BaseCommandSender(IMediator mediator)
{
    protected readonly IMediator Mediator = mediator;

    /// <summary>
    /// 基类虚方法
    /// </summary>
    public virtual async Task<string> BaseMethod(CancellationToken cancellationToken = default)
    {
        return await Mediator.Send(new RecordCommandWithResult("Base Method"), cancellationToken);
    }

    /// <summary>
    /// 抽象方法，子类必须实现
    /// </summary>
    public abstract Task AbstractMethod(CancellationToken cancellationToken = default);
}

/// <summary>
/// 继承自基类的CommandSender
/// </summary>
public class DerivedCommandSender(IMediator mediator) : BaseCommandSender(mediator)
{
    /// <summary>
    /// 重写基类方法
    /// </summary>
    public override async Task<string> BaseMethod(CancellationToken cancellationToken = default)
    {
        await Mediator.Send(new ClassCommandWithOutResult { Name = "Derived Override" }, cancellationToken);
        return await base.BaseMethod(cancellationToken);
    }

    /// <summary>
    /// 实现抽象方法
    /// </summary>
    public override async Task AbstractMethod(CancellationToken cancellationToken = default)
    {
        await Mediator.Send(new RecordCommandWithResult("Abstract Implementation"), cancellationToken);
    }

    /// <summary>
    /// 派生类特有方法
    /// </summary>
    public async Task<string> DerivedSpecificMethod(CancellationToken cancellationToken = default)
    {
        return await Mediator.Send(new RecordCommandWithResult("Derived Specific"), cancellationToken);
    }
}

/// <summary>
/// 测试接口实现的CommandSender
/// </summary>
public interface ICommandSenderInterface
{
    Task<string> InterfaceMethod(CancellationToken cancellationToken = default);
}

public class InterfaceImplementationCommandSender(IMediator mediator) : ICommandSenderInterface
{
    /// <summary>
    /// 实现接口方法
    /// </summary>
    public async Task<string> InterfaceMethod(CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new RecordCommandWithResult("Interface Implementation"), cancellationToken);
    }

    /// <summary>
    /// 显式接口实现
    /// </summary>
    async Task<string> ICommandSenderInterface.InterfaceMethod(CancellationToken cancellationToken)
    {
        await mediator.Send(new ClassCommandWithOutResult { Name = "Explicit Interface" }, cancellationToken);
        return "Explicit Implementation";
    }
}
