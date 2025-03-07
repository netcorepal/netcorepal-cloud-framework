namespace NetCorePal.Extensions.DistributedTransactions.UnitTests.Assembly2;

public class MockIntegrationEvent2
{
}

[IntegrationEventConsumer(nameof(MockIntegrationEvent2), "abc")]
public class MockIntegrationEventHandler : IIntegrationEventHandler<MockIntegrationEvent2>
{
    public Task HandleAsync(MockIntegrationEvent2 eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

[IntegrationEventConsumer(nameof(MockIntegrationEvent2), "abc")]
public class MockIntegrationEventHandler2 : IIntegrationEventHandler<MockIntegrationEvent2>
{
    public Task HandleAsync(MockIntegrationEvent2 eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}