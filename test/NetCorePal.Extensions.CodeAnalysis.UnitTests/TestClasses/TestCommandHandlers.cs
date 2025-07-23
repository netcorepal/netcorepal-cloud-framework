using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public class TestCommandHandlerWithOutResult : ICommandHandler<RecordCommandWithOutResult>
{
    public Task Handle(RecordCommandWithOutResult request, CancellationToken cancellationToken)
    {
        // 处理逻辑
        return Task.CompletedTask;
    }
}

public class TestCommandHandlerWithResult : ICommandHandler<RecordCommandWithResult,string>
{
    public Task<string> Handle(RecordCommandWithResult request, CancellationToken cancellationToken)
    {
        // 处理逻辑
        return Task.FromResult(request.Name);
    }
}
