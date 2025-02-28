# Integration Events

Since `domain events` are only used within local transactions, we need a mechanism to transmit events in a distributed system so that the handling of events does not block the execution of the `command` that initiated the event. This is the role of `integration events`.

## Publish Integration Events

Integration events are converted from `domain events` and are generally named with the suffix `IntegrationEvent` to distinguish them from `domain events`, such as `OrderCreatedIntegrationEvent`.

We usually publish `integration events` in the handler of `domain events`, such as:

```csharp

public record OrderCreatedDomainEvent(OrderId OrderId) : IDomainEvent;

public class OrderCreatedDomainEventHandler(IIntegrationEventPublisher integrationEventPublisher) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return integrationEventPublisher.Publish(new OrderCreatedIntegrationEvent(notification.Order.Id));
    }
}
```

## Integration Event Handling

An integration event handler is a class that implements the `IIntegrationEventHandler<TIntegrationEvent>` interface, where `TIntegrationEvent` is the type of the integration event.

Generally, we can do the following in the integration event handler:

+ Send commands
+ Call external services

Here is an example of an integration event handler:

```csharp
public class OrderCreatedIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task Handle(OrderCreatedIntegrationEvent eventData, CancellationToken cancellationToken)
    {
        // Handle integration event
        var cmd = new OrderPaidCommand(eventData.OrderId);
        await mediator.Send(cmd, cancellationToken);
    }
}
```

## Retry on Failure

We use the `CAP` component to implement integration events. The `CAP` component provides a retry mechanism for failures. When the integration event handling fails, `CAP` will automatically retry. By default, it will retry `10` times.

## Limitations

Integration events must use simple objects because integration events need to support JSON format serialization and deserialization and be transmitted in a distributed system.