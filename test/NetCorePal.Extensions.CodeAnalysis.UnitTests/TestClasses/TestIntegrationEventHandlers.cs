using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public class TestAggregateRootNameChangedIntegrationEventHandler : IIntegrationEventHandler<TestAggregateRootNameChangedIntegrationEvent>
{
    public Task HandleAsync(TestAggregateRootNameChangedIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class TestPrivateMethodIntegrationEventHandler : IIntegrationEventHandler<TestPrivateMethodIntegrationEvent>
{
    public Task HandleAsync(TestPrivateMethodIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

[IntegrationEventConsumer(nameof(TestPrivateMethodIntegrationEvent2),groupName: "TestGroup")]
public class TestPrivateMethodIntegrationEventHandler2 : IIntegrationEventHandler<TestPrivateMethodIntegrationEvent2>
{
    public Task HandleAsync(TestPrivateMethodIntegrationEvent2 eventData, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}