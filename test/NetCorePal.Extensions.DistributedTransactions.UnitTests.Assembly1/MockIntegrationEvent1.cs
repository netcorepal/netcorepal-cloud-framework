namespace NetCorePal.Extensions.DistributedTransactions.UnitTests.Assembly1;

public class MockIntegrationEvent1
{
}

public class MockIntegrationEventHandler : IIntegrationEventHandler<MockIntegrationEvent1>
{
    public Task HandleAsync(MockIntegrationEvent1 eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}


public class MockIntegrationEventHandler2 : IIntegrationEventHandler<MockIntegrationEvent1>
{
    public Task HandleAsync(MockIntegrationEvent1 eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}