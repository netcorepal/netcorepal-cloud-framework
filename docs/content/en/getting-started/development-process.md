# Quick Development Process

This section introduces the main development process using this framework to help you get started quickly.

## 1. Create Domain Model

Create domain models in the domain layer, defining the properties, methods, events, and rules of the domain models.

```csharp
// Define domain model
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

// Define strongly-typed ID for the model
public partial record UserId : IInt64StronglyTypedId;

// Domain model
public class User : Entity<UserId>, IAggregateRoot
{
    protected User() { }

    public User(string name, string email)
    {
        Name = name;
        Email = email;
        this.AddDomainEvent(new UserCreatedDomainEvent(this));
    }
    public string Name { get; private set; }
    public string Email { get; private set; }

    public void ChangeEmail(string email)
    {
        Email = email;
        this.AddDomainEvent(new UserEmailChangedDomainEvent(this));
    }
}
```

Define domain events

```csharp
// Define domain events
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public record UserCreatedDomainEvent(User user) : IDomainEvent;
```

## 2. Create Repository

Define the repository interface for the domain model

```csharp
// Define repository interface
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User> GetByEmailAsync(string email); // Optional custom query method
}
```

Implement the repository interface

```csharp
// Implement repository interface
using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
namespace YourNamespace;

public class UserRepository : RepositoryBase<User, UserId, ApplicationDbContext>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await DbContext.Set<User>().FirstOrDefaultAsync(x => x.Email == email);
    }
}
```

## 3. Define Model and Database Mapping

```csharp
// Define model and database mapping
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Domain;

namespace YourNamespace;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(50);
    }
}
```

## 4. Commands and Command Handlers

Define commands for the domain model

```csharp
// Define commands for the domain model
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public record CreateUserCommand(string Name, string Email) : ICommand<UserId>;
```

Define command handlers for the domain model

```csharp
// Define command handlers for the domain model
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class CreateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> HandleAsync(CreateUserCommand command)
    {
        var user = new User(command.Name, command.Email);
        await userRepository.AddAsync(user);
        return user.Id;
    }
}
```

## 5. Define Web API Interface

Use the IMediator interface in the endpoint to handle commands for the domain model

Define RequestDto

```csharp
// Define RequestDto
namespace YourNamespace;

public record CreateUserRequestDto(string Name, string Email);
```

Define ResponseDto

```csharp
// Define ResponseDto
namespace YourNamespace;

public record CreateUserResponseDto(UserId UserId);
```

Define FastEndpoint

```csharp
// Define Endpoint
using FastEndpoints;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Dto;
namespace YourNamespace;

public class CreateUserEndpoint(IMediator mediator) : Endpoint<CreateUserRequestDto, ResponseData<CreateUserResponseDto>>
{
    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
    {
        var command = new CreateUserCommand(req.Name, req.Email);
        var userId = await mediator.SendAsync(command, ct);
        var response = new CreateUserResponseDto(userId).AsResponseData();
        await SendAsync(response, cancellation: ct);
    }
}
```

## 6. Write Integration Tests

Write integration tests to test the command handlers of the domain model, using `MyWebApplicationFactory` to create the test environment

```csharp
// Write integration tests
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using YourNamespace;
namespace YourNamespace.Tests;

public class CreateUserEndpointTests : IClassFixture<MyWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateUserEndpointTests(MyWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUserTest()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new CreateUserRequestDto("test", "abc@efg.com"));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResponseData<CreateUserResponseDto>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }   
}
```

## 7. Domain Event Handlers

Define domain event handlers

```csharp
// Define domain event handlers
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class UserCreatedDomainEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task HandleAsync(UserCreatedDomainEvent domainEvent)
    {
        // Handle domain event, send points to the points domain
    }
}
```

## 8. Define Integration Events

Define integration events

```csharp
// Define integration events
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public record UserCreatedIntegrationEvent(UserId userId) : IIntegrationEvent;
```

## 9. Publish Integration Events

Use the integration event converter to convert domain events to integration events, the framework will automatically publish integration events:

```csharp
public class UserCreatedIntegrationEventConverter : IIntegrationEventConverter<UserCreatedDomainEvent,UserCreatedIntegrationEvent>{
    public UserCreatedIntegrationEvent Convert(UserCreatedDomainEvent domainEvent)
    {
        return new UserCreatedIntegrationEvent(domainEvent.User.Id);
    }
}
```

Note: IIntegrationEventPublisher is no longer recommended, the framework will automatically publish integration events

~~Use IIntegrationEventPublisher to publish integration events~~

```csharp
// Publish integration events
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class UserCreatedDomainEventHandler(IIntegrationEventPublisher integrationEventPublisher) : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task HandleAsync(UserCreatedDomainEvent domainEvent)
    {
        // Handle domain event
    }
}
```

## 10. Integration Event Handlers

Define integration event handlers

```csharp
// Define integration event handlers
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class UserCreatedIntegrationEventHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    public async Task HandleAsync(UserCreatedIntegrationEvent integrationEvent)
    {
        // Handle integration event
        var cmd = new AddUserScoreCommand(integrationEvent.UserId);
        await _mediator.SendAsync(cmd);
    }
}
```
