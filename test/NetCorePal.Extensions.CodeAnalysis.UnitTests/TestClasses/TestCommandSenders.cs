using MediatR;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

public class TestCommandSender(IMediator mediator)
{
    public Task SendWithConstraints(CancellationToken cancellationToken = default)
    {
        return mediator.Send(new RecordCommandWithOutResult("Test Name"), cancellationToken);
    }

    public Task SendUseParams(CancellationToken cancellationToken = default)
    {
        var cmd = new RecordCommandWithOutResult("Test Name");
        return mediator.Send(cmd, cancellationToken);
    }

    public Task<string> SendWithResult(CancellationToken cancellationToken = default)
    {
        return mediator.Send(new RecordCommandWithResult("Test Name"), cancellationToken);
    }
}