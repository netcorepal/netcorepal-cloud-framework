# 快速开发流程

这里介绍了使用本框架的主要开发流程，以帮助您快速上手。

## 1. 创建领域模型

在领域层创建领域模型，定义领域模型的属性、方法、事件、规则等。

```csharp
// 定义领域模型
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

//为模型定义强类型ID
public partial record UserId : IInt64StronglyTypedId;

//领域模型
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
    public string Email { get; set; }
}
```

定义领域事件

```csharp
//定义领域事件
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public record UserCreatedDomainEvent(User user) : IDomainEvent;
```

## 2. 创建仓储

定义领域模型的仓储接口

```csharp
//定义仓储接口
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User> GetByEmailAsync(string email); //可选的自定义查询方法
}
```

实现仓储接口

```csharp
//实现仓储接口
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

## 3. 定义模型与数据库映射关系

```csharp
//定义模型与数据库映射关系
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


## 4. 命令与命令处理器

定义领域模型的命令

```csharp

//定义领域模型的命令
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public record CreateUserCommand(string Name, string Email) : ICommand<User>;
```

定义领域模型的命令处理器

```csharp
//定义领域模型的命令处理器
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class CreateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<CreateUserCommand, User>
{
    public async Task<User> HandleAsync(CreateUserCommand command)
    {
        var user = new User(command.Name, command.Email);
        await userRepository.AddAsync(user);
        return user;
    }
}
```

## 5. 定义webapi接口

在controller中使用IMediator接口处理领域模型的命令

定义RequestDto

```csharp
//定义RequestDto
namespace YourNamespace;

public record CreateUserRequestDto(string Name, string Email);
```

定义ResponseDto

```csharp
//定义ResponseDto
namespace YourNamespace;

public record CreateUserResponseDto(long Id, string Name, string Email);
```

定义Controller

```csharp
//定义Controller
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

[ApiController]
[Route("api/[controller]")]

public class UserController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequestDto requestDto)
    {
        var command = new CreateUserCommand(requestDto.Name, requestDto.Email);
        var user = await _mediator.SendAsync(command);
        return new CreateUserResponseDto(user.Id, user.Name, user.Email).AsResponseData();
    }
}
```

## 6. 编写集成测试

编写集成测试，测试领域模型的命令处理器,使用`MyWebApplicationFactory`来创建测试环境
    
```csharp
//编写集成测试
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


## 7. 领域事件处理器

定义领域事件处理器

```csharp

//定义领域事件处理器
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class UserCreatedDomainEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task HandleAsync(UserCreatedDomainEvent domainEvent)
    {
        //处理领域事件,发送积分 积分领域

    }
}
```

## 8. 定义集成事件

定义集成事件

```csharp
//定义集成事件
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public record UserCreatedIntegrationEvent(UserId userId) : IIntegrationEvent;
```

## 9. 发出集成事件

使用 IIntegrationEventPublisher 发出集成事件

```csharp
//发出集成事件
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class UserCreatedDomainEventHandler(IIntegrationEventPublisher integrationEventPublisher) : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task HandleAsync(UserCreatedDomainEvent domainEvent)
    {
        //处理领域事件
        await integrationEventPublisher.PublishAsync(new UserCreatedIntegrationEvent(domainEvent.User.Id));
    }
}
```

# 10. 集成事件处理器

定义集成事件处理器

```csharp
//定义集成事件处理器
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class UserCreatedIntegrationEventHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    public async Task HandleAsync(UserCreatedIntegrationEvent integrationEvent)
    {
        //处理集成事件
    }
}
```
