using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.CodeAnalysis.Example
{
    /// <summary>
    /// 代码流分析示例程序
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== NetCorePal 代码流分析示例 ===");
            Console.WriteLine();

            // 显示项目统计信息
            DisplayStatistics();
            
            Console.WriteLine();
            
            // 显示 Controller 到 Command 的调用关系
            DisplayControllerToCommandRelationships();
            
            Console.WriteLine();
            
            // 显示 Entity 方法创建的 DomainEvent
            DisplayEntityToDomainEventRelationships();
            
            Console.WriteLine();
            
            // 显示 DomainEventHandler 发出的 Command
            DisplayDomainEventHandlerToCommandRelationships();
            
            Console.WriteLine();
            
            // 显示完整的调用链
            DisplayCompleteCallChains();
            
            Console.WriteLine();
            Console.WriteLine("分析完成。按任意键退出...");
            Console.ReadKey();
        }

        private static void DisplayStatistics()
        {
            Console.WriteLine("📊 项目统计信息:");
            Console.WriteLine("=" * 50);
            
            // 这些类将由源生成器在编译时生成
            #if false // 这部分代码展示了源生成器将生成的内容
            var summary = CodeFlowStatistics.GetSummary();
            Console.WriteLine(summary);
            
            Console.WriteLine($"Controllers: {CodeFlowStatistics.ControllerCount}");
            Console.WriteLine($"Commands: {CodeFlowStatistics.CommandCount}");
            Console.WriteLine($"Command Handlers: {CodeFlowStatistics.CommandHandlerCount}");
            Console.WriteLine($"Entities: {CodeFlowStatistics.EntityCount}");
            Console.WriteLine($"Aggregate Roots: {CodeFlowStatistics.AggregateRootCount}");
            Console.WriteLine($"Domain Events: {CodeFlowStatistics.DomainEventCount}");
            Console.WriteLine($"Domain Event Handlers: {CodeFlowStatistics.DomainEventHandlerCount}");
            Console.WriteLine($"Total Relationships: {CodeFlowStatistics.RelationshipCount}");
            #endif
            
            // 示例输出
            Console.WriteLine("Controllers: 3");
            Console.WriteLine("Commands: 8");
            Console.WriteLine("Command Handlers: 8");
            Console.WriteLine("Entities: 5");
            Console.WriteLine("Aggregate Roots: 2");
            Console.WriteLine("Domain Events: 4");
            Console.WriteLine("Domain Event Handlers: 4");
            Console.WriteLine("Total Relationships: 18");
        }

        private static void DisplayControllerToCommandRelationships()
        {
            Console.WriteLine("🎯 Controller -> Command 调用关系:");
            Console.WriteLine("=" * 50);
            
            #if false // 源生成器将生成这些方法
            var orderCommands = EnhancedCodeFlowAnalysis.GetCommandsCalledByController("OrderController");
            Console.WriteLine("OrderController 调用的命令:");
            foreach (var command in orderCommands)
            {
                Console.WriteLine($"  ➤ {command}");
            }
            
            var userCommands = EnhancedCodeFlowAnalysis.GetCommandsCalledByController("UserController");
            Console.WriteLine("\nUserController 调用的命令:");
            foreach (var command in userCommands)
            {
                Console.WriteLine($"  ➤ {command}");
            }
            #endif
            
            // 示例输出
            Console.WriteLine("OrderController 调用的命令:");
            Console.WriteLine("  ➤ CreateOrderCommand");
            Console.WriteLine("  ➤ UpdateOrderCommand");
            Console.WriteLine("  ➤ DeleteOrderCommand");
            
            Console.WriteLine("\nUserController 调用的命令:");
            Console.WriteLine("  ➤ CreateUserCommand");
            Console.WriteLine("  ➤ UpdateUserCommand");
        }

        private static void DisplayEntityToDomainEventRelationships()
        {
            Console.WriteLine("🏗️ Entity 方法 -> DomainEvent 创建关系:");
            Console.WriteLine("=" * 50);
            
            #if false // 源生成器将生成这些方法
            var orderEvents = EnhancedCodeFlowAnalysis.GetDomainEventsCreatedByEntityMethod("Order", "Create");
            Console.WriteLine("Order.Create() 创建的领域事件:");
            foreach (var eventType in orderEvents)
            {
                Console.WriteLine($"  ➤ {eventType}");
            }
            
            var orderUpdateEvents = EnhancedCodeFlowAnalysis.GetDomainEventsCreatedByEntityMethod("Order", "UpdateStatus");
            Console.WriteLine("\nOrder.UpdateStatus() 创建的领域事件:");
            foreach (var eventType in orderUpdateEvents)
            {
                Console.WriteLine($"  ➤ {eventType}");
            }
            #endif
            
            // 示例输出
            Console.WriteLine("Order.Create() 创建的领域事件:");
            Console.WriteLine("  ➤ OrderCreatedDomainEvent");
            
            Console.WriteLine("\nOrder.UpdateStatus() 创建的领域事件:");
            Console.WriteLine("  ➤ OrderStatusChangedDomainEvent");
            
            Console.WriteLine("\nUser.Create() 创建的领域事件:");
            Console.WriteLine("  ➤ UserCreatedDomainEvent");
        }

        private static void DisplayDomainEventHandlerToCommandRelationships()
        {
            Console.WriteLine("⚡ DomainEventHandler -> Command 发出关系:");
            Console.WriteLine("=" * 50);
            
            #if false // 源生成器将生成这些方法
            var handlerCommands = EnhancedCodeFlowAnalysis.GetCommandsIssuedByDomainEventHandler("OrderCreatedDomainEventHandler");
            Console.WriteLine("OrderCreatedDomainEventHandler 发出的命令:");
            foreach (var command in handlerCommands)
            {
                Console.WriteLine($"  ➤ {command}");
            }
            #endif
            
            // 示例输出
            Console.WriteLine("OrderCreatedDomainEventHandler 发出的命令:");
            Console.WriteLine("  ➤ SendWelcomeEmailCommand");
            Console.WriteLine("  ➤ CreateDeliveryCommand");
            
            Console.WriteLine("\nUserCreatedDomainEventHandler 发出的命令:");
            Console.WriteLine("  ➤ SendWelcomeEmailCommand");
            Console.WriteLine("  ➤ InitializeUserProfileCommand");
        }

        private static void DisplayCompleteCallChains()
        {
            Console.WriteLine("🔗 完整调用链:");
            Console.WriteLine("=" * 50);
            
            Console.WriteLine("示例调用链 1:");
            Console.WriteLine("OrderController.CreateOrder()");
            Console.WriteLine("  ↓ mediator.Send()");
            Console.WriteLine("CreateOrderCommand -> CreateOrderCommandHandler");
            Console.WriteLine("  ↓ order.Create()");
            Console.WriteLine("Order.Create()");
            Console.WriteLine("  ↓ AddDomainEvent()");
            Console.WriteLine("OrderCreatedDomainEvent");
            Console.WriteLine("  ↓ 触发");
            Console.WriteLine("OrderCreatedDomainEventHandler");
            Console.WriteLine("  ↓ mediator.Send()");
            Console.WriteLine("SendWelcomeEmailCommand -> SendWelcomeEmailCommandHandler");
            
            Console.WriteLine();
            
            Console.WriteLine("示例调用链 2:");
            Console.WriteLine("UserController.Register()");
            Console.WriteLine("  ↓ mediator.Send()");
            Console.WriteLine("CreateUserCommand -> CreateUserCommandHandler");
            Console.WriteLine("  ↓ user.Create()");
            Console.WriteLine("User.Create()");
            Console.WriteLine("  ↓ AddDomainEvent()");
            Console.WriteLine("UserCreatedDomainEvent");
            Console.WriteLine("  ↓ 触发");
            Console.WriteLine("UserCreatedDomainEventHandler");
            Console.WriteLine("  ↓ mediator.Send()");
            Console.WriteLine("InitializeUserProfileCommand -> InitializeUserProfileCommandHandler");
        }

        private static void DisplayMermaidDiagram()
        {
            Console.WriteLine("📊 Mermaid 流程图:");
            Console.WriteLine("=" * 50);
            
            #if false // 源生成器将生成 Mermaid 代码
            var mermaidCode = CodeFlowMermaidDiagram.FlowChart;
            Console.WriteLine(mermaidCode);
            #endif
            
            // 示例 Mermaid 代码
            Console.WriteLine(@"
graph TD
    OrderController[OrderController] --> CreateOrderCommand[CreateOrderCommand]
    CreateOrderCommand --> Order[Order Entity]
    Order --> OrderCreatedDomainEvent[OrderCreatedDomainEvent]
    OrderCreatedDomainEvent --> OrderCreatedDomainEventHandler[OrderCreatedDomainEventHandler]
    OrderCreatedDomainEventHandler --> SendWelcomeEmailCommand[SendWelcomeEmailCommand]
    
    UserController[UserController] --> CreateUserCommand[CreateUserCommand]
    CreateUserCommand --> User[User Entity]
    User --> UserCreatedDomainEvent[UserCreatedDomainEvent]
    UserCreatedDomainEvent --> UserCreatedDomainEventHandler[UserCreatedDomainEventHandler]
    UserCreatedDomainEventHandler --> InitializeUserProfileCommand[InitializeUserProfileCommand]
            ");
        }

        private static void DisplayJsonOutput()
        {
            Console.WriteLine("📄 JSON 输出示例:");
            Console.WriteLine("=" * 50);
            
            #if false // 源生成器将生成 JSON 数据
            var jsonData = CodeFlowAnalysisJson.Data;
            Console.WriteLine(jsonData);
            #endif
            
            // 示例 JSON 输出
            var exampleJson = @"{
  ""Controllers"": [
    {
      ""Type"": {
        ""Name"": ""OrderController"",
        ""FullName"": ""MyApp.Controllers.OrderController"",
        ""Namespace"": ""MyApp.Controllers""
      },
      ""Actions"": [
        {
          ""Name"": ""CreateOrder"",
          ""HttpMethod"": ""POST"",
          ""RouteTemplate"": ""/api/orders"",
          ""Commands"": [""MyApp.Commands.CreateOrderCommand""]
        }
      ]
    }
  ],
  ""Relationships"": [
    {
      ""Source"": {
        ""TypeName"": ""MyApp.Controllers.OrderController"",
        ""MethodName"": ""CreateOrder""
      },
      ""Target"": {
        ""TypeName"": ""MyApp.Commands.CreateOrderCommand"",
        ""MethodName"": ""Handle""
      },
      ""CallType"": ""ControllerToCommand""
    }
  ]
}";
            Console.WriteLine(exampleJson);
        }
    }
}

#region 示例代码结构 - 这些是源生成器会分析的典型代码

// 示例 Controller
/*
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request.Name, request.Price);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
*/

// 示例 Command
/*
public record CreateOrderCommand(string Name, decimal Price) : ICommand<OrderId>;
*/

// 示例 CommandHandler
/*
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderId>
{
    private readonly IOrderRepository _repository;
    
    public CreateOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<OrderId> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = new Order(command.Name, command.Price);
        await _repository.AddAsync(order);
        return order.Id;
    }
}
*/

// 示例 Entity
/*
public class Order : Entity<OrderId>, IAggregateRoot
{
    protected Order() { }
    
    public Order(string name, decimal price)
    {
        Name = name;
        Price = price;
        this.AddDomainEvent(new OrderCreatedDomainEvent(this));
    }
    
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        this.AddDomainEvent(new OrderPriceChangedDomainEvent(this, newPrice));
    }
}
*/

// 示例 DomainEvent
/*
public record OrderCreatedDomainEvent(Order Order) : IDomainEvent;
*/

// 示例 DomainEventHandler
/*
public class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private readonly IMediator _mediator;
    
    public OrderCreatedDomainEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var command = new SendWelcomeEmailCommand(notification.Order.Id);
        await _mediator.Send(command, cancellationToken);
    }
}
*/

#endregion
