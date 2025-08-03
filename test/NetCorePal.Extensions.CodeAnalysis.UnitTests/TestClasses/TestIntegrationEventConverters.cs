using NetCorePal.Extensions.DistributedTransactions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 订单创建领域事件到集成事件转换器
/// </summary>
public class TestAggregateRootNameChangedIntegrationEventConverter : IIntegrationEventConverter<TestAggregateRootNameChangedDomainEvent, TestAggregateRootNameChangedIntegrationEvent>
{
    public TestAggregateRootNameChangedIntegrationEvent Convert(TestAggregateRootNameChangedDomainEvent domainEvent)
    {
        return new TestAggregateRootNameChangedIntegrationEvent(domainEvent.AggregateRoot.Name);
    }
}


public class TestPrivateMethodIntegrationEventConverter : IIntegrationEventConverter<TestPrivateMethodDomainEvent, TestPrivateMethodIntegrationEvent>
{
    public TestPrivateMethodIntegrationEvent Convert(TestPrivateMethodDomainEvent domainEvent)
    {
        return new TestPrivateMethodIntegrationEvent(domainEvent.AggregateRoot.Name);
    }
}

public class TestPrivateMethodIntegrationEventConverter2 : IIntegrationEventConverter<TestPrivateMethodDomainEvent, TestPrivateMethodIntegrationEvent2>
{
    public TestPrivateMethodIntegrationEvent2 Convert(TestPrivateMethodDomainEvent domainEvent)
    {
        return new TestPrivateMethodIntegrationEvent2(domainEvent.AggregateRoot.Name);
    }
}