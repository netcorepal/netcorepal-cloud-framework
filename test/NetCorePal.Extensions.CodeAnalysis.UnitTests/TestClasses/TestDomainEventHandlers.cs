using MediatR;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public class TestAggregateRootNameChangedDomainEventHandler : INotificationHandler<TestAggregateRootNameChangedDomainEvent>
{
    public Task Handle(TestAggregateRootNameChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Handle the domain event
        return Task.CompletedTask;
    }
}

public class TestPrivateMethodDomainEventHandler : INotificationHandler<TestPrivateMethodDomainEvent>
{
    public Task Handle(TestPrivateMethodDomainEvent notification, CancellationToken cancellationToken)
    {
        // Handle the domain event
        return Task.CompletedTask;
    }
}

public class TestPrivateMethodDomainEventHandler2 : INotificationHandler<TestPrivateMethodDomainEvent>
{
    public Task Handle(TestPrivateMethodDomainEvent notification, CancellationToken cancellationToken)
    {
        // Handle the domain event
        return Task.CompletedTask;
    }
}   