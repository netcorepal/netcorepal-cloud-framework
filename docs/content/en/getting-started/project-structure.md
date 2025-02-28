# Project Structure

## Overall Structure
```text
YourProject
     ├──src
     │  ├──YourProject.Web  // Web project
     │  ├──YourProject.Domain // Domain model
     │  └──YourProject.Infrastructure // Infrastructure
     └──tests
        ├──YourProject.Web.Tests  // Web tests
        ├──YourProject.Domain.Tests // Domain model tests
        └──YourProject.Infrastructure.Tests // Infrastructure tests
```

## Domain Model Layer Project Structure
    
```text
YourProject.Domain
     ├── AggregatesModel // Domain model directory, containing aggregate roots, entities, value objects, etc.
     └── DomainEvents // Domain events directory
```

## Infrastructure Layer Project Structure

```text
YourProject.Infrastructure
     ├── EntityConfigurations  // Domain model database mapping configurations directory
     ├── Repositories  // Repositories directory
     └── ApplicationDbContext.cs  // Database context
```

## Web Layer Project Structure

```text
YourProject.Web
     ├── wwwroot // Static resources directory
     ├── Application  // Application services directory
     │   ├── Commands  // Commands, command handlers, command validators directory
     │   ├── DomainEventHandlers  // Domain event handlers directory
     │   ├── IntegrationEventConverters // Integration event converters directory
     │   ├── IntegrationEventHandlers  // Integration event handlers directory
     │   └── Queries  // Query services directory
     ├── Clients  // Remote service clients directory, for accessing other microservices or third-party services
     ├── Controllers  // Controllers directory
     ├── Extensions  // Extension methods directory, containing various extension method definitions
     ├── Filters  // Filters directory
     └── Program.cs  // Startup entry class
```