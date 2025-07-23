using MediatR;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public class TestController
{
    private readonly IMediator _mediator;

    public TestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 测试命令发送
    /// </summary>
    public async Task<string> SendRecordCommandWithResult()
    {
        return await _mediator.Send(new RecordCommandWithResult("TestName"));
    }

    /// <summary>
    /// 测试无结果命令发送
    /// </summary>
    public async Task SendRecordCommandWithOutResult()
    {
        var cmd = new RecordCommandWithOutResult("TestName");
        await _mediator.Send(cmd);
    }

    public Task MethodWithOutCommand()
    {
        // 这个方法没有发出任何命令
        return Task.CompletedTask;
    }
}

public class TestWithPrimaryConstructorsController(IMediator mediator)
{
    /// <summary>
    /// 测试命令发送
    /// </summary>
    public async Task<string> SendClassCommandWithResult()
    {
        var command = new ClassCommandWithResult { Name = "TestName" };
        return await mediator.Send(command);
    }

    /// <summary>`
    /// 测试无结果命令发送
    /// </summary>
    public async Task SendClassCommandWithOutResult()
    {
        await mediator.Send(new ClassCommandWithOutResult { Name = "TestName" });
    }

    public async Task<string> SendMultiCommand()
    {
        await mediator.Send(new ClassCommandWithOutResult { Name = "TestName" });
        return await mediator.Send(new RecordCommandWithResult("TestName"));
    }
}


