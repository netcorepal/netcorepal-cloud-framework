# 测试类优化说明

## 优化目标

减少重复的链路，创建更清晰的测试场景，专注于核心的业务流程。

## 优化后的结构

### 聚合根 (Aggregates)
- **Order.cs** - 订单聚合根，包含核心业务方法
- **User.cs** - 用户聚合根，包含核心业务方法
- **OrderItem.cs** - 订单项实体（非聚合根），包含方法发出领域事件（如 UpdateQuantity 发出 OrderItemQuantityUpdatedDomainEvent）

### 命令 (Commands)
- **OrderCommands.cs** - 订单相关命令，包含 UpdateOrderItemQuantityCommand
  - `CreateOrderCommand` - 创建订单
  - `OrderPaidCommand` - 订单支付
  - `ChangeOrderNameCommand` - 更改订单名称
  - `DeleteOrderCommand` - 删除订单
- **UserCommands.cs** - 用户相关命令
  - `CreateUserCommand` - 创建用户
  - `ActivateUserCommand` - 激活用户
  - `DeactivateUserCommand` - 禁用用户

### 控制器 (Controllers)
- **OrderController.cs** - 订单控制器，包含核心API方法
- **UserController.cs** - 用户控制器，包含核心API方法

### 命令处理器 (Command Handlers)
- **OrderCommandHandlers.cs** - 订单命令处理器，包含 UpdateOrderItemQuantityCommandHandler，调用子实体方法
- **UserCommandHandlers.cs** - 用户命令处理器

### 领域事件 (Domain Events)
- **OrderDomainEvents.cs** - 包含所有领域事件定义
  - `OrderCreatedDomainEvent` - 订单创建
  - `OrderPaidDomainEvent` - 订单支付
  - `OrderNameChangedDomainEvent` - 订单名称变更
  - `OrderDeletedDomainEvent` - 订单删除
  - `UserCreatedDomainEvent` - 用户创建
  - `UserActivatedDomainEvent` - 用户激活
  - `UserDeactivatedDomainEvent` - 用户禁用
  - `OrderItemAddedDomainEvent` - 订单项添加

### 领域事件处理器 (Domain Event Handlers)
- **OrderDomainEventHandlers.cs** - 订单领域事件处理器，OrderPaidDomainEventHandler 同时发多个命令
  - `OrderCreatedDomainEventHandler` - 处理订单创建事件，发送创建用户命令
  - `OrderPaidDomainEventHandler` - 处理订单支付事件，发送激活用户命令

### 集成事件 (Integration Events)
- **OrderIntegrationEvents.cs** - 订单集成事件
- **ExternalSystemNotificationEvent.cs** - 外部系统通知事件
- **UserRegisteredIntegrationEvent.cs** - 用户注册集成事件

### 集成事件处理器 (Integration Event Handlers)
- **OrderIntegrationEventHandlers.cs** - 订单集成事件处理器，OrderPaidIntegrationEventHandler 同时发多个命令
- **ExternalSystemNotificationHandler.cs** - 外部系统通知处理器

### 集成事件转换器 (Integration Event Converters)
- **OrderIntegrationEventConverters.cs** - 订单集成事件转换器，包含多对多领域事件与集成事件转换器

### 服务 (Services)
- **OrderProcessingService.cs** - 订单处理服务，包含 ManualSendCommand 方法，模拟任意类型方法发命令

### 端点 (Endpoints)
- **OrderEndpoints.cs** - 订单API端点
- **UserEndpoints.cs** - 用户API端点

## 核心链路场景

### 1. 订单创建链路
```
OrderController.CreateOrder() 
→ CreateOrderCommand 
→ CreateOrderCommandHandler 
→ Order (聚合根) 
→ OrderCreatedDomainEvent 
→ OrderCreatedDomainEventHandler 
→ CreateUserCommand 
→ CreateUserCommandHandler 
→ User (聚合根)
```

### 2. 订单支付链路
```
OrderController.PayOrder() 
→ OrderPaidCommand 
→ OrderPaidCommandHandler 
→ Order.MarkAsPaid() 
→ OrderPaidDomainEvent 
→ OrderPaidDomainEventHandler 
→ ActivateUserCommand 
→ ActivateUserCommandHandler 
→ User.Activate()
```

### 3. 领域事件到集成事件转换
```
OrderCreatedDomainEvent 
→ OrderCreatedIntegrationEventConverter 
→ OrderCreatedIntegrationEvent 
→ OrderCreatedIntegrationEventHandler 
→ CreateUserCommand
```

## 优化效果
1. **减少重复链路** - 移除了大量重复的OrderItem操作方法
2. **简化测试场景** - 专注于核心业务流程3. **清晰的链路模式** - 每个链路都有明确的业务含义
4. **易于维护** - 代码结构更清晰，便于理解和修改

## 链路规则

- **聚合根**：Order和User作为聚合根，包含核心业务逻辑
- **命令模式**：Controller → Command → CommandHandler → Aggregate
- **事件驱动**：DomainEvent → DomainEventHandler → Command
- **集成事件**：DomainEvent → IntegrationEventConverter → IntegrationEvent → IntegrationEventHandler
- **服务层**：OrderProcessingService作为命令发送者，处理复杂业务流程 