using MediatR;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

public class TestAggregateRootNameChangedDomainEventHandler(IMediator mediator)
    : IDomainEventHandler<TestAggregateRootNameChangedDomainEvent>
{
    public async Task Handle(TestAggregateRootNameChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new TestAggregateRootNameChangedDomainEventHandlerCommand1(), cancellationToken);
        var command2 = new TestAggregateRootNameChangedDomainEventHandlerCommand2();
        await mediator.Send(command2, cancellationToken);
    }
}

public class TestPrivateMethodDomainEventHandler : IDomainEventHandler<TestPrivateMethodDomainEvent>
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