using MediatR;
using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public class TestAggregateRootNameChangedIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<TestAggregateRootNameChangedIntegrationEvent>
{
    public Task HandleAsync(TestAggregateRootNameChangedIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 使用构造函数参数发出命令
        return mediator.Send(new TestIntegrationEventCommand(eventData.Name), cancellationToken);
    }
}

public class TestPrivateMethodIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<TestPrivateMethodIntegrationEvent>
{
    public Task HandleAsync(TestPrivateMethodIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        // 使用变量的方式发出命令
        var cmd = new TestIntegrationEventCommand(eventData.Name);
        return mediator.Send(cmd, cancellationToken);
    }
}

[IntegrationEventConsumer(nameof(TestPrivateMethodIntegrationEvent2), groupName: "TestGroup")]
public class TestPrivateMethodIntegrationEventHandler2 : IIntegrationEventHandler<TestPrivateMethodIntegrationEvent2>
{
    public Task HandleAsync(TestPrivateMethodIntegrationEvent2 eventData, CancellationToken cancellationToken = default)
    {
        //不发出命令
        throw new NotImplementedException();
    }
}

[IntegrationEventConsumer(nameof(TestPrivateMethodIntegrationEvent2), groupName: "TestGroup2")]
public class TestPrivateMethodIntegrationEventHandler3(IMediator mediator) : IIntegrationEventHandler<TestPrivateMethodIntegrationEvent2>
{
    public async Task HandleAsync(TestPrivateMethodIntegrationEvent2 eventData, CancellationToken cancellationToken = default)
    {
        //发出两个命令
        await mediator.Send(new TestIntegrationEventCommand(eventData.Name), cancellationToken);
        await mediator.Send(new TestIntegrationEventCommand2(eventData.Name), cancellationToken);
    }
}