# Domain Event Handling

A domain event handler is a processing logic that handles `domain events` for a specific purpose. A domain event handler should handle domain events for only one purpose. Different purposes for the same `domain event` should have different domain event handlers.

## Define Domain Event Handler

1. Install the nuget package `NetCorePal.Extensions.Domain.Abstractions`

    ```bash
    dotnet add package NetCorePal.Extensions.Domain.Abstractions
    ```

2. A domain event handler is a class that implements the `IDomainEventHandler<TDomainEvent>` interface, where `TDomainEvent` is the type of the domain event.

Here is an example of a domain event handler:

    ```csharp
    public class OrderCreatedDomainEventHandler(IMediator mediator) : IDomainEventHandler<OrderCreatedDomainEvent>
    {
        public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            return mediator.Send(new DeliverGoodsCommand(notification.Order.Id), cancellationToken);
        }
    }
    ```

## Domain Event Handlers Must

- Domain event handlers must be idempotent, meaning that processing the same domain event multiple times should result in the same outcome;
- A single domain event handler can only issue one command and is not allowed to issue multiple commands within the same handler;

**Note: In our framework, domain event handlers are executed synchronously, and the commands they call are in the same transaction as the CommandHandler that triggers the domain event.**

## Domain Event Handlers Can

- A single domain event can correspond to multiple domain event handlers;
- Domain event handlers can use the `MediatR` framework to send commands;
- Domain event handlers can retrieve data from multiple queries;
- Domain event handlers can call external services to complete some information organization and validation;
- Domain event handlers can publish integration events to transmit events to other systems;

## Domain Event Handlers Should Not

- Domain event handlers should not contain operations on domain models. Domain models should be operated and persisted by CommandHandlers.

**Note: Since our framework only manages transactions for `CommandHandlers`, operations in domain event handlers will not be saved to the database because the framework does not call `SaveChangesAsync` for them.**