# 集成事件

由于`领域事件`仅在本地事务中使用，所以我们需要一种机制来在分布式系统中传递事件，使得事件的处理不阻断发起事件的`命令`的执行，这就是`集成事件`的作用。

## 发出集成事件

集成事件是由`领域事件`转换而来的，命名一般以`IntegrationEvent`结尾从而与`领域事件`区分，如`OrderCreatedIntegrationEvent`；

一般我们会在`领域事件`的处理器中发出`集成事件`，如：

```csharp

public record OrderCreatedDomainEvent(OrderId OrderId) : IDomainEvent;

public class OrderCreatedDomainEventHandler(IIntegrationEventPublisher integrationEventPublisher) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return integrationEventPublisher.Publish(new OrderCreatedIntegrationEvent(notification.Order.Id));
    }
}
```

## 集成事件处理

集成事件处理器是一个实现了`IIntegrationEventHandler<TIntegrationEvent>`接口的类，其中`TIntegrationEvent`是集成事件的类型。

一般我们可以在集成事件处理器中做下列事情：

+ 发出命令
+ 调用外部服务

下面是一个集成事件处理器的例子：

```csharp
public class OrderCreatedIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public async Task Handle(OrderCreatedIntegrationEvent eventData, CancellationToken cancellationToken)
    {
        //处理集成事件
        var cmd = new OrderPaidCommand(eventData.OrderId);
        await mediator.Send(cmd, cancellationToken);
    }
}
```

## 失败重试

我们借助`CAP`组件实现了集成事件，`CAP`组件提供了失败重试机制，当集成事件处理失败时，`CAP`会自动重试处理，默认情况下会重试`10`次。

## 限制条件

集成事件必须使用简单对象，因为集成事件会在需要支持`Json`格式序列化和反序列化并在分布式系统中传递。