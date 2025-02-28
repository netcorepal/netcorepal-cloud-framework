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
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class UserRepository : Repository<User, UserId>, IUserRepository
{
    public UserRepository(IDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await this.DbSet.FirstOrDefaultAsync(x => x.Email == email);
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

Use the IMediator interface in the controller to handle commands for the domain model

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

Define Controller

```csharp
// Define Controller
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

[ApiController]
[Route("api/[controller]")]

public class UserController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ResponseData<CreateUserResponseDto>> CreateUser(CreateUserRequestDto requestDto)
    {
        var command = new CreateUserCommand(requestDto.Name, requestDto.Email);
        var userId = await _mediator.SendAsync(command);
        return new CreateUserResponseDto(userId).AsResponseData();
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

public class UserControllerTests : IClassFixture<MyWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserControllerTests(MyWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUserTest()
    {
        var response = await _client.PostAsJsonAsync("/api/User", new CreateUserRequestDto("test", "abc@efg.com"));
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<CreateUserResponseDto>();
        Assert.NotNull(user);
        Assert.Equal("test", user.Name);
        Assert.Equal("abc@efg.com", user.Email);
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

Use IIntegrationEventPublisher to publish integration events

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
