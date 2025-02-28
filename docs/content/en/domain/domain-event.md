# Domain Event

Domain events are an important concept in domain models. They are a communication mechanism within the domain model, used to pass messages between domain models.

Domain events only contain data describing the state of the domain model when the event occurs. They do not contain any business logic, business rules, or business processes.

## Define Domain Events

1. Install the nuget package `NetCorePal.Extensions.Domain.Abstractions`

    ```bash
    dotnet add package NetCorePal.Extensions.Domain.Abstractions
    ```

2. To define a domain event, you need to:

    + Inherit from the `NetCorePal.Extensions.Domain.IDomainEvent` interface;
    + Define an empty constructor for the domain event to support serialization and deserialization;
    + Define a public constructor for the domain event to initialize the properties of the domain event;
    + Define public properties for the domain event to describe the state of the domain model when the event occurs;
    
    Here is an example:

    ```csharp
    // Define domain event
    using NetCorePal.Extensions.Domain;
    namespace YourNamespace;
   
    public record UserCreatedDomainEvent(User user) : IDomainEvent;
    ```

## Domain Events Must

- Domain events must be issued by the domain model;
- Domain events must be immutable;

## Domain Events Can

- Use the `record` keyword to define domain events;

## Domain Events Should Not

- Do not contain business logic
- Do not contain business rules
- Do not contain business processes