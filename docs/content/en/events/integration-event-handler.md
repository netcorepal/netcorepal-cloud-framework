# Integration Events

Since `domain events` are only used in local transactions, we need a mechanism to transmit events in a distributed system so that the handling of events does not block the execution of the `command` that initiated the event. This is the role of `integration events`.

## Register Integration Event Services

The framework currently implements the `CAP` component to support integration events. We need to register the `CAP` component in the `Startup` class:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCap(x =>
    {
        x.UseEntityFramework<AppDbContext>();
        x.UseRabbitMQ("localhost");
    });
}

// Configure CAP for integration events
builder.Services.AddIntegrationEvents(typeof(Program))
        .UseCap(b =>
        {
            b.RegisterServicesFromAssemblies(typeof(Program));
            b.AddContextIntegrationFilters();
            b.UseMySql();
        });

```

## Emitting Integration Events

Integration events are converted from `domain events` and are generally named with the suffix `IntegrationEvent` to distinguish them from `domain events`, such as `OrderCreatedIntegrationEvent`.

To emit an integration event, we need to define an `IIntegrationEventConverter`, which the framework will automatically use to convert domain events into integration events and emit them.

```csharp
public class OrderCreatedIntegrationEventConverter : 
    IIntegrationEventConverter<OrderCreatedDomainEvent, OrderCreatedIntegrationEvent>
{
    public OrderCreatedIntegrationEvent Convert(OrderCreatedDomainEvent domainEvent)
    {
        return new OrderCreatedIntegrationEvent(domainEvent.Order.Id);
    }
}
```

## Integration Event Handling

An integration event handler is a class that implements the `IIntegrationEventHandler<TIntegrationEvent>` interface, where `TIntegrationEvent` is the type of the integration event.

Typically, we can do the following in an integration event handler:

+ Emit commands
+ Call external services

Here is an example of an integration event handler:

```csharp
public class OrderCreatedIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task Handle(OrderCreatedIntegrationEvent eventData, CancellationToken cancellationToken)
    {
        // Handle the integration event
        var cmd = new OrderPaidCommand(eventData.OrderId);
        await mediator.Send(cmd, cancellationToken);
    }
}
```

## Retry on Failure

We use the `CAP` component to implement integration events. The `CAP` component provides a retry mechanism for failures. When the handling of an integration event fails, `CAP` will automatically retry the handling. By default, it will retry `10` times.

## Limitations

Integration events must use simple objects because integration events need to support JSON format serialization and deserialization and be transmitted in a distributed system.