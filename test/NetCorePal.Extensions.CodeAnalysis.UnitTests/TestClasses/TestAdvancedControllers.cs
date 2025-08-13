using MediatR;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 测试没有发出任何命令的Controller
/// </summary>
public class TestEmptyController
{
    private readonly IMediator _mediator;

    public TestEmptyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 只读取数据，不发出任何命令
    /// </summary>
    public Task<string> GetDataOnly()
    {
        // 只返回数据，不发出命令
        return Task.FromResult("Data");
    }

    /// <summary>
    /// 没有任何操作的方法
    /// </summary>
    public void DoNothing()
    {
        // 空方法
    }
}

/// <summary>
/// 测试复杂命令发送场景的Controller
/// </summary>
public class TestComplexController(IMediator mediator)
{
    /// <summary>
    /// 在条件分支中发出不同的命令
    /// </summary>
    public async Task<string> ConditionalCommandSend(bool condition)
    {
        if (condition)
        {
            return await mediator.Send(new RecordCommandWithResult("Condition True"));
        }
        else
        {
            await mediator.Send(new ClassCommandWithOutResult { Name = "Condition False" });
            return "Done";
        }
    }

    /// <summary>
    /// 在循环中发出命令
    /// </summary>
    public async Task SendCommandsInLoop(string[] names)
    {
        foreach (var name in names)
        {
            await mediator.Send(new RecordCommandWithResult(name));
        }
    }

    /// <summary>
    /// 异步并行发出多个命令
    /// </summary>
    public async Task SendCommandsInParallel()
    {
        var tasks = new List<Task>
        {
            mediator.Send(new RecordCommandWithResult("Task1")),
            mediator.Send(new ClassCommandWithOutResult { Name = "Task2" }),
            mediator.Send(new RecordCommandWithResult("Task3"))
        };
        
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 在try-catch块中发出命令
    /// </summary>
    public async Task<string> SendCommandWithErrorHandling()
    {
        try
        {
            return await mediator.Send(new RecordCommandWithResult("ErrorTest"));
        }
        catch (Exception)
        {
            await mediator.Send(new ClassCommandWithOutResult { Name = "ErrorHandling" });
            throw;
        }
    }
}

/// <summary>
/// 测试泛型Controller
/// </summary>
public class TestGenericController<T>(IMediator mediator) where T : class
{
    /// <summary>
    /// 泛型方法发出命令
    /// </summary>
    public async Task<string> ProcessGenericData(T data)
    {
        return await mediator.Send(new RecordCommandWithResult($"Generic: {typeof(T).Name}"));
    }
}

/// <summary>
/// 测试继承的Controller
/// </summary>
public abstract class BaseTestController(IMediator mediator)
{
    protected readonly IMediator Mediator = mediator;

    /// <summary>
    /// 基类方法发出命令
    /// </summary>
    public virtual async Task<string> BaseMethod()
    {
        return await Mediator.Send(new RecordCommandWithResult("Base"));
    }
}

/// <summary>
/// 继承自基类的Controller
/// </summary>
public class DerivedTestController(IMediator mediator) : BaseTestController(mediator)
{
    /// <summary>
    /// 重写基类方法
    /// </summary>
    public override async Task<string> BaseMethod()
    {
        await Mediator.Send(new ClassCommandWithOutResult { Name = "Derived" });
        return await base.BaseMethod();
    }

    /// <summary>
    /// 派生类特有的方法
    /// </summary>
    public async Task<string> DerivedMethod()
    {
        return await Mediator.Send(new RecordCommandWithResult("Derived Specific"));
    }
}
