# Domain Model

## Introduction

A domain model is an abstraction and modeling representation of a specific domain or business in software development. It describes the entities, concepts, relationships, and behaviors within the domain and provides an interactive and executable representation. Domain models are typically implemented using object-oriented programming concepts such as classes, objects, properties, and methods.

In Domain-Driven Design (DDD), the domain model is emphasized as a core component to guide the design and implementation of the system. DDD encourages development teams to work closely with domain experts to explore and understand the complexity of the domain and translate these understandings into maintainable and scalable software systems.

The domain model in Domain-Driven Design is created by modeling the knowledge of domain experts. It is not only used for problem domain analysis and design but also for implementing business logic in software systems. The domain model is closely related to Domain-Driven Design because it is one of the core elements of DDD, helping developers understand and address complex business scenarios to build high-quality software systems.

## Defining a Domain Model

To define a domain model, follow these steps:

1. Install the NuGet package `NetCorePal.Extensions.Domain.Abstractions`

    ```bash
    dotnet add package NetCorePal.Extensions.Domain.Abstractions
    ```

2. Define the domain model, which requires:

    + Defining a strongly-typed ID for the model (optional), the model type can be defined as needed;
    + Inheriting from the `NetCorePal.Extensions.Domain.Entity<T>` class and specifying the model ID type;
    + Implementing the `IAggregateRoot` interface (optional), only needed if the model is defined as an aggregate root, allowing the model to be used as a generic parameter for repositories;
    + Defining a protected parameterless constructor for the model to support the EntityFrameworkCore framework in constructing domain model instances during queries;

    Here is an example:

    ```csharp
    // Define the domain model
    using NetCorePal.Extensions.Domain;
    namespace YourNamespace;

    // Define a strongly-typed ID for the model
    public partial record UserId : IInt64StronglyTypedId;
    
    // Domain model
    public class User : Entity<UserId>, IAggregateRoot
    {
        protected User() { }
        public string Name { get; private set; }
        public string Email { get; set; }
    }
    ```
   
## Domain Model Requirements

- The properties of the domain model must be read-only externally and writable internally (private set);
- Changes to the properties of the domain model must be made through instance methods of the model;
- The domain model must have a `protected` parameterless constructor to support the EntityFrameworkCore framework in constructing domain model instances during queries;
- The domain model must inherit from the `IEntity` or `Entity<TKey>` class;
- The methods of the domain model must be called by `CommandHandler`;

## Domain Model Options

- The domain model can implement the `IAggregateRoot` interface to indicate that the model is an aggregate root;
- The properties and methods of the domain model are used to describe the entities, concepts, relationships, and behaviors within the domain;
- The constructor of the domain model is used to initialize the properties of the domain model;
- The events of the domain model are used to describe the state changes of the domain model;
- The rules of the domain model are used to validate the legality of the domain model;

## Domain Model Don'ts

- Do not reference external resources such as database connections, file systems, etc., in the domain model;
- Do not handle logic unrelated to business such as logging, exception handling, etc., in the domain model;
- Do not directly call external services such as Web API, message queues, etc., in the domain model;
- Do not handle data unrelated to business such as configuration information, environment variables, etc., in the domain model;
- Do not handle states unrelated to business such as session information, user information, etc., in the domain model;
- Do not handle behaviors unrelated to business such as tracking, monitoring, etc., in the domain model;
- Do not handle exceptions unrelated to business such as network exceptions, database exceptions, etc., in the domain model;
- Do not handle events unrelated to business such as scheduled tasks, message notifications, etc., in the domain model;
